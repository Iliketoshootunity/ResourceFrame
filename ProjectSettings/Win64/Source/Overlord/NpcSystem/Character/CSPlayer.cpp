// Fill out your copyright notice in the Description page of Project Settings.


#include "CSPlayer.h"
#include "Components/InputComponent.h"
#include "SkillSystem/CSSkillDefine.h"
#include "SkillSystem/CSSkillComponent.h"
#include "SkillSystem/CSSkill.h"
#include "BaseSystem/FSM/FSMState.h"
#include "BaseSystem/FSM/FSMMachine.h"
#include "BaseSystem/Ext/MathfExtLibrary.h"
#include "Kismet/GameplayStatics.h"
#include "Kismet/KismetMathLibrary.h"
#include "BaseSystem/Ext/ActorInterpMovementComponent.h"
#include "SGameInstance.h"
#include "GameFramework/Character.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "../Other/CSPlayerSearchEnemyComponent.h"


ACSPlayer::ACSPlayer()
{

}



void ACSPlayer::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	check(PlayerInputComponent);
	PlayerInputComponent->BindAxis("MoveForward", this, &ACSPlayer::MoveForward);
	PlayerInputComponent->BindAxis("MoveRight", this, &ACSPlayer::MoveRight);

	PlayerInputComponent->BindAction("NormalAtk", IE_Pressed, this, &ACSPlayer::NormalAttackPressed);
	PlayerInputComponent->BindAction("NormalAtk", IE_Released, this, &ACSPlayer::NormalAttackRelesed);
	PlayerInputComponent->BindAction("Skill1Atk", IE_Pressed, this, &ACSPlayer::Skill1Pressed);
	PlayerInputComponent->BindAction("Skill1Atk", IE_Released, this, &ACSPlayer::Skill1Relesed);
}

void ACSPlayer::MoveForward(float Val)
{
	RockerY = Val;
}

void ACSPlayer::MoveRight(float Val)
{
	RockerX = Val;
	if (RockerX != 0.f || RockerY != 0.f)
	{
		if (MotionMachine->ChangeState((int32)EMotionMachineState::Run, (int32)EMotionMachineTransition::ToRun))
		{
			FVector2D RockerInput = FVector2D(RockerX, RockerY);
			RockerSize = RockerInput.Size();
			if (RockerSize <= 0.5f)
			{
				RockerSize = 0.5f;
			}
			else
			{
				RockerSize = 1;
			}
			float Angle = UMathfExtLibrary::VectorAngle2D(RockerInput, FVector2D(0, 1));
			if (RockerX < 0)Angle = -Angle;
			const FRotator Rotation = GetControlRotation();
			const FRotator YawRotation(0, Rotation.Yaw + Angle, 0);
			const FVector Direction = UKismetMathLibrary::GetForwardVector(YawRotation);
			AddMovementInput(Direction, RockerSize);

			AddControllerYawInput(RockerX * 0.15f);
			//正常行走不允许非常规的运动形式
			InterpMovement->StopMovementImmediately();
		}

	}
	else
	{
		if (MotionMachine->GetCurrentStateID() == (int32)EMotionMachineState::Run)
		{
			float VelocitySize = GetCharacterMovement()->Velocity.Size();
			if (VelocitySize >= 250)
			{
				//播放
				FString MontageFilePath = "/Game/Character/Hero/Anim/Montage/RunToIdle";		// test path
				UObject* LoadObj = StaticLoadObject(UAnimMontage::StaticClass(), this, *MontageFilePath);
				if (LoadObj)
				{
					UAnimMontage* RunToIdleMontage = Cast<UAnimMontage>(LoadObj);
					//PlayAnimMontage(RunToIdleMontage, 1, "");
				}
			}
			RockerSize = 0;
			MotionMachine->ChangeState((int32)EMotionMachineState::Idle, (int32)EMotionMachineTransition::ToIdle);
		}

	}
}

void ACSPlayer::NormalAttackPressed()
{
	if (Skill)
	{
		Skill->RespondInput(ESkillButtonType::NormalAttackBtn, EInputStatus::Press, 10000);
	}
}

void ACSPlayer::NormalAttackRelesed()
{
	if (Skill)
	{
		Skill->RespondInput(ESkillButtonType::NormalAttackBtn, EInputStatus::Relese, 10000);
	}
}


void ACSPlayer::Skill1Pressed()
{
	if (Skill)
	{
		Skill->RespondInput(ESkillButtonType::SkillAttackBtn, EInputStatus::Press, 10002);
	}
}

void ACSPlayer::Skill1Relesed()
{
	if (Skill)
	{
		Skill->RespondInput(ESkillButtonType::SkillAttackBtn, EInputStatus::Relese, 10002);
	}
}

void ACSPlayer::OnRotateToTagrget(float RoateSpeed, float MaxAngle, float RotateTime /*= 0*/)
{
	float SourceYaw = CacheSourceYaw;
	float TargetYaw = 0;
	float Time = 0;
	if (CacheTargetYaw != -9999)
	{
		TargetYaw = CacheTargetYaw;
		Time = RotateTime;
	}
	else
	{
		if (CacheRockerX == 0 && CacheRockerY == 0)
		{
			return;
		}
		//计算目标旋转
		float ControllerYaw = CacheControllerYaw;
		float NewControllerYaw = 0;
		UKismetMathLibrary::FMod(ControllerYaw, 360, NewControllerYaw);
		float Angle = UMathfExtLibrary::VectorAngle2D(FVector2D(CacheRockerX, CacheRockerY), FVector2D(0, 1));
		if (CacheRockerX < 0)
		{
			Angle = -Angle;
		}
		float TargetYawTemp = NewControllerYaw + Angle;
		UKismetMathLibrary::FMod(TargetYawTemp, 360, TargetYaw);
		//同向
		float DifYaw = TargetYaw - SourceYaw;
		if (UKismetMathLibrary::Abs(DifYaw) > 180)
		{
			if (TargetYaw > 0)
			{
				TargetYaw = TargetYaw - 360;
			}
			else
			{
				TargetYaw = 360 + TargetYaw;
			}
		}
		//角度限制
		DifYaw = TargetYaw - SourceYaw;
		if (UKismetMathLibrary::Abs(DifYaw) > MaxAngle)
		{
			if (DifYaw > 0)
			{
				TargetYaw = SourceYaw + MaxAngle;
			}
			else
			{
				TargetYaw = SourceYaw - MaxAngle;
			}
		}
		Time = UKismetMathLibrary::Abs(SourceYaw - TargetYaw) / RoateSpeed;
	}
	//旋转
	InterpMovement->StartRotateToTagetYaw(TargetYaw, Time);
}

void ACSPlayer::CoverOperation(float NewRockerX, float NewRockerY, float NewSourceYaw, float NewControllerYaw, float NewTargetYaw)
{
	CacheRockerX = NewRockerX;
	CacheRockerY = NewRockerY;
	CacheSourceYaw = NewSourceYaw;
	CacheControllerYaw = NewControllerYaw;
	CacheTargetYaw = NewTargetYaw;
}

void ACSPlayer::OnComboEnded(UAnimMontage* Montage, bool bInterrupted)
{
	Super::OnComboEnded(Montage, bInterrupted);
	if (IsMainPlayer() && !bInterrupted)
	{
		SendComboEndMessage();
	}
}

void ACSPlayer::PrepareComboClip()
{
	//查找敌人，缓存朝向数据
	if (IsMainPlayer() && SearchEnemy)
	{
		UCSPlayerSearchEnemyComponent* PlayerSearhEnemy = Cast<UCSPlayerSearchEnemyComponent>(SearchEnemy);
		if (PlayerSearhEnemy)
		{
			PlayerSearhEnemy->SetUpParameters(500, 300, 180, 180, 180, FVector2D(RockerX, RockerY));
			AActor* Enemy = PlayerSearhEnemy->ExecuteSearchEnemy();
			if (Enemy && PlayerSearhEnemy->GetTargetDirection() != FRotator::ZeroRotator)
			{
				float EndYaw = PlayerSearhEnemy->GetTargetDirection().Yaw;
				CacheTargetYaw = EndYaw;
			}
			else
			{
				CacheTargetYaw = -9999;
			}
		}
		//缓存操作数据
		CacheRockerX = RockerX;
		CacheRockerY = RockerY;
		CacheSourceYaw = GetActorRotation().Yaw;
		CacheControllerYaw = GetControlRotation().Yaw;
	}
}



void ACSPlayer::MoveMessage(FVector Target, float MaxWalkSpeed)
{
	moveV2::ReqMove WalkMesaage;
	WalkMesaage.set_x(GetActorLocation().X);
	WalkMesaage.set_y(GetActorLocation().Y);
	WalkMesaage.set_z(GetActorLocation().Z);
	WalkMesaage.set_ox(Target.X);
	WalkMesaage.set_oy(Target.Y);
	WalkMesaage.set_oz(Target.Z);
	int32 Action = 0;
	if (MaxWalkSpeed == 300)
	{
		Action = 1;
	}
	else if (MaxWalkSpeed == 600)
	{
		Action = 2;
	}
	WalkMesaage.set_action(Action);
	g_pGameInstance->SendMessage(68001, &WalkMesaage);

}

void ACSPlayer::SendMoveMessage(float DeltaTime)
{
	if (RockerX == 0 && RockerY == 0)
	{
		if (OldMoveMessagePosition != FVector::ZeroVector)
		{
			MoveMessage(GetActorLocation(), CacheMaxSpeed);
			OldMoveMessagePosition = FVector::ZeroVector;
		}
	}
	else
	{
		FVector TargetPosition = CalMoveMessageTarget();
		float Angle = UMathfExtLibrary::VectorAngle2D(FVector2D(TargetPosition.X, TargetPosition.Y), FVector2D(OldMoveMessagePosition.X, OldMoveMessagePosition.Y));
		bool IsSend = false;
		if (Angle > 0.1f)
		{
			IsSend = true;
		}
		else
		{
			MoveMessageTimer -= DeltaTime;
			if (MoveMessageTimer < 0)
			{
				IsSend = true;
			}
		}
		if (IsSend)
		{
			float NewMaxSpeed = 0;
			if (RockerSize <= 0.5f)
			{
				NewMaxSpeed = CacheMaxSpeed * 0.5f;
			}
			else if (RockerSize <= 1)
			{
				NewMaxSpeed = CacheMaxSpeed;
			}
			MoveMessage(TargetPosition, NewMaxSpeed);
			MoveMessageTimer = 0.5f;
			OldMoveMessagePosition = TargetPosition;
		}
	}
}

void ACSPlayer::SendComboMessage(int32 ComboId)
{
	fightV2::ComboNode ComboNode;
	ComboNode.set_combonode(ComboId);
	ComboNode.set_dir(GetActorRotation().Yaw);
	ComboNode.set_x(GetActorLocation().X);
	ComboNode.set_y(GetActorLocation().Y);
	ComboNode.set_z(GetActorLocation().Z);
	ComboNode.set_rockerx(CacheRockerX);
	ComboNode.set_rockery(CacheRockerY);
	ComboNode.set_controlleryaw(CacheControllerYaw);
	ComboNode.set_targetdir(CacheTargetYaw);
	g_pGameInstance->SendMessage(69014, &ComboNode);
}


void ACSPlayer::SendComboEndMessage()
{
	fightV2::ComboEnd ComboEnd;
	g_pGameInstance->SendMessage(69016, &ComboEnd);
}



