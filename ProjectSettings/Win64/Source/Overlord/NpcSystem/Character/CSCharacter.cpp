// Fill out your copyright notice in the Description page of Project Settings.


#include "CSCharacter.h"
#include "Components/SkeletalMeshComponent.h"
#include "GameFramework/Pawn.h"
#include "Animation/AnimMontage.h"
#include "BasicFunction.h"
#include "AIController.h"
#include "Components/InputComponent.h"
#include "Animation/AnimInstance.h"
#include "Animation/AnimMontage.h"
#include "GameFramework/Character.h"
#include "BaseSystem/FSM/FSMState.h"
#include "BaseSystem/FSM/FSMMachine.h"
#include "GameFramework/SpringArmComponent.h"
#include "SkillSystem/CSSkillDefine.h"
#include "SkillSystem/CSSkillComponent.h"
#include "SkillSystem/CSSkill.h"
#include "GameFramework/Character.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "BaseSystem/Ext/ActorInterpMovementComponent.h"
#include "Kismet/KismetMathLibrary.h"
#include "BaseSystem/GameUtil.h"
#include "WidgetComponent.h"
#include "../Other/CSSearchEnemyComponent.h"
#include "../Other/CSHead.h"
#include "../GameCharacter/CSGameCharacter.h"
#include "SkillSystem/CSComboClip.h"
#include "SGameInstance.h"
#include "../Weapon/CSWeapon.h"
#include "../Other/CSHurtResult.h"
#include "Kismet/GameplayStatics.h"
#include "WorldSystem/SWorldManager.h"


// Sets default values
ACSCharacter::ACSCharacter()
{
	// Set this character to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
	InterpMovement = CreateDefaultSubobject<UActorInterpMovementComponent>(TEXT("ActorInterpMovement"));
	Skill = CreateDefaultSubobject<UCSSkillComponent>(TEXT("Skill"));

}

// Called when the game starts or when spawned
void ACSCharacter::BeginPlay()
{
	Super::BeginPlay();

	//获取组件
	UClass* CameraBoomClass = USpringArmComponent::StaticClass();
	CameraBoom = Cast<USpringArmComponent>(GetComponentByClass(CameraBoomClass));

	UClass* SearchEnemyClass = UCSSearchEnemyComponent::StaticClass();
	SearchEnemy = Cast<UCSSearchEnemyComponent>(GetComponentByClass(SearchEnemyClass));

	UClass* WidgetClass = UWidgetComponent::StaticClass();
	HeadWidget = Cast<UWidgetComponent>(GetComponentByClass(WidgetClass));


	//构建运动状态机
	ConstructionMotionMachine();

	ToIdle();

}

void ACSCharacter::BeginDestroy()
{
	if (Skill)
	{
		Skill->Destroy();
	}

	Super::BeginDestroy();

}

// Called every frame
void ACSCharacter::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}


void ACSCharacter::Init(UCSGameCharacter* InCharacterInfo)
{
	if (InCharacterInfo == nullptr)return;
	CharacterInfo = InCharacterInfo;
	Head = NewObject<UCSHead>(this);
	Head->Init(CharacterInfo);
}


void ACSCharacter::InterpMoveToTarget(FVector TargetRot, float Time)
{
	InterpMovement->StartMoveToTarget(TargetRot, Time);
}

void ACSCharacter::StopMove()
{
	InterpMovement->StopMovementImmediately();
	GetCharacterMovement()->StopMovementImmediately();
}


float ACSCharacter::PlayAnimMontage(class UAnimMontage* AnimMontage, float InPlayRate /* = 1.f */, FName StartSectionName /* = NAME_None */)
{
	USkeletalMeshComponent* UseMesh = GetPawnMesh();
	if (AnimMontage && UseMesh && UseMesh->AnimScriptInstance)
	{
		return UseMesh->AnimScriptInstance->Montage_Play(AnimMontage, InPlayRate);
	}
	return 0;
}

void ACSCharacter::PlayMontage(FString MontageName, UAnimMontage*& OutAnimMontag, FOnMontageEnded EndDelegate)
{
	//播放蒙太奇
	FString MontageFilePath = MontageName;
	UObject* LoadObj = StaticLoadObject(UAnimMontage::StaticClass(), this, *MontageFilePath);
	if (LoadObj)
	{
		OutAnimMontag = Cast<UAnimMontage>(LoadObj);
		PlayAnimMontage(OutAnimMontag, 1, "");
		//绑定结束事件
		USkeletalMeshComponent* UseMesh = GetPawnMesh();
		if (UseMesh && UseMesh->AnimScriptInstance)
		{
			UseMesh->AnimScriptInstance->Montage_SetEndDelegate(EndDelegate, OutAnimMontag);
		}
	}
	else
	{
		FBasicFunction::Log("Montage no load montage", true);
		return;
	}
	return;
}


void ACSCharacter::StopAnimMontage(UAnimMontage* AnimMontage)
{
	if (AnimMontage == nullptr)return;
	USkeletalMeshComponent* UseMesh = GetPawnMesh();
	if (AnimMontage && UseMesh && UseMesh->AnimScriptInstance &&
		UseMesh->AnimScriptInstance->Montage_IsPlaying(AnimMontage))
	{
		UseMesh->AnimScriptInstance->Montage_Stop(AnimMontage->BlendOut.GetBlendTime(), AnimMontage);
	}
}

void ACSCharacter::StopAllAnimMontages()
{
	USkeletalMeshComponent* UseMesh = GetPawnMesh();
	if (UseMesh && UseMesh->AnimScriptInstance)
	{
		UseMesh->AnimScriptInstance->Montage_Stop(0.0f);
	}
}


int32 ACSCharacter::GetAnimationMode()
{
	if (!GetPawnMesh())return -1;

	return GetPawnMesh()->GetAnimationMode();
}

USkeletalMeshComponent* ACSCharacter::GetPawnMesh() const
{
	return GetMesh();
}

bool ACSCharacter::IsMainPlayer() const
{
	if (CharacterInfo->GetCharacterType() == ECharaterType::MainPlayer)
	{
		return true;
	}
	return false;
}

bool ACSCharacter::IsAlive() const
{
	return true;
}


void ACSCharacter::SetSkeletalMesh(USkeletalMesh* NewSkeletalMesh, FVector Scale3D, EAnimationMode::Type AnimMode, FString AnimInstanceName, TArray<FMaterialData> Materials, bool bReinitPose /*= true*/)
{
	if (!GetMesh() || !NewSkeletalMesh) return;

	GetMesh()->SetSkeletalMesh(NewSkeletalMesh, bReinitPose);
	GetMesh()->SetRelativeScale3D(Scale3D);
	GetMesh()->SetAnimationMode(AnimMode);

	UClass* LoadAnimClass = UGameUtil::LoadClass<UClass>(AnimInstanceName);
	if (LoadAnimClass)
	{
		GetMesh()->SetAnimInstanceClass(LoadAnimClass);
	}

	for (auto MaterialData : Materials)
	{
		UMaterialInterface* pMaterial = UGameUtil::LoadClass<UMaterialInterface>(MaterialData.MaterialName);
		if (!pMaterial) continue;
		GetMesh()->SetMaterial(MaterialData.nSlot, pMaterial);
	}
}


void ACSCharacter::AdjustCameraBoomArmLength(float MinLength, float MaxLength, float Speed)
{
	if (CameraBoom)
	{
		float DeltaSeconds = 0.02f;
		float TargetLength = DeltaSeconds * Speed + CameraBoom->TargetArmLength;
		if (TargetLength < MaxLength)
		{
			if (TargetLength < MinLength)
			{
				CameraBoom->TargetArmLength = MinLength;
			}
			else
			{
				CameraBoom->TargetArmLength = TargetLength;
			}
		}
		else
		{
			CameraBoom->TargetArmLength = MaxLength;
		}
	}
}


void ACSCharacter::MoveToTargetPosition(FVector TargetPostion, float MaxSpeed)
{
	if (MotionMachine->ChangeState((int32)EMotionMachineState::Run, (int32)EMotionMachineTransition::ToRun))
	{
		float Distance = UKismetMathLibrary::Vector_Distance(TargetPostion, GetActorLocation());
		if (Distance > 100)
		{
			//立马转向目标
			FRotator NewRotate = UKismetMathLibrary::FindLookAtRotation(GetActorLocation(), TargetPostion);
			SetActorRotation(FRotator(GetActorRotation().Pitch, NewRotate.Yaw, GetActorRotation().Roll));
		}
		GetCharacterMovement()->MaxWalkSpeed = MaxSpeed;
		AAIController* AC = Cast<AAIController>(GetController());
		if (AC == nullptr)return;
		AC->MoveToLocation(TargetPostion, 0);
	}
}


void ACSCharacter::ConstructionMotionMachine()
{
	MotionMachine = NewObject<UFSMMachine>(this);
	if (MotionMachine != NULL)
	{
		UFSMState* IdleState = NewObject<UFSMState>(MotionMachine);
		IdleState->Init((int32)EMotionMachineState::Idle);
		IdleState->AddTransition((int32)EMotionMachineTransition::ToRoll, (int32)EMotionMachineState::Roll);
		IdleState->AddTransition((int32)EMotionMachineTransition::ToRun, (int32)EMotionMachineState::Run);
		IdleState->AddTransition((int32)EMotionMachineTransition::ToSkill, (int32)EMotionMachineState::Skill);
		IdleState->AddTransition((int32)EMotionMachineTransition::ToHurt, (int32)EMotionMachineState::Hurt);


		UFSMState* RunState = NewObject<UFSMState>(MotionMachine);
		RunState->Init((int32)EMotionMachineState::Run);
		RunState->AddTransition((int32)EMotionMachineTransition::ToIdle, (int32)EMotionMachineState::Idle);
		RunState->AddTransition((int32)EMotionMachineTransition::ToSkill, (int32)EMotionMachineState::Skill);
		RunState->AddTransition((int32)EMotionMachineTransition::ToRoll, (int32)EMotionMachineState::Roll);
		RunState->AddTransition((int32)EMotionMachineTransition::ToHurt, (int32)EMotionMachineState::Hurt);

		UFSMState* RollState = NewObject<UFSMState>(MotionMachine);
		RollState->Init((int32)EMotionMachineState::Roll);
		RollState->AddTransition((int32)EMotionMachineTransition::ToRun, (int32)EMotionMachineState::Run);
		RollState->AddTransition((int32)EMotionMachineTransition::ToIdle, (int32)EMotionMachineState::Idle);
		RollState->AddTransition((int32)EMotionMachineTransition::ToRun, (int32)EMotionMachineState::Skill);
		RollState->AddTransition((int32)EMotionMachineTransition::ToHurt, (int32)EMotionMachineState::Hurt);

		UFSMState* SkillState = NewObject<UFSMState>(MotionMachine);
		SkillState->Init((int32)EMotionMachineState::Skill);
		SkillState->AddTransition((int32)EMotionMachineTransition::ToIdle, (int32)EMotionMachineState::Idle);
		SkillState->AddTransition((int32)EMotionMachineTransition::ToRoll, (int32)EMotionMachineState::Roll);
		SkillState->AddTransition((int32)EMotionMachineTransition::ToHurt, (int32)EMotionMachineState::Hurt);

		UFSMState* HurtState = NewObject<UFSMState>(MotionMachine);
		HurtState->Init((int32)EMotionMachineState::Hurt);
		HurtState->AddTransition((int32)EMotionMachineTransition::ToIdle, (int32)EMotionMachineState::Idle);
		HurtState->AddTransition((int32)EMotionMachineTransition::ToDead, (int32)EMotionMachineState::Dead);

		UFSMState* DeadState = NewObject<UFSMState>(MotionMachine);
		DeadState->Init((int32)EMotionMachineState::Dead);

		MotionMachine->AddState(IdleState);
		MotionMachine->AddState(RollState);
		MotionMachine->AddState(RunState);
		MotionMachine->AddState(SkillState);
		MotionMachine->AddState(HurtState);
		MotionMachine->AddState(DeadState);
	}
}

void ACSCharacter::ToIdle()
{
	MotionMachine->ChangeState((int32)EMotionMachineState::Idle, (int32)EMotionMachineTransition::ToIdle);
}

void ACSCharacter::ToHurt(UCSHurtResult* HurtResult)
{
	if (HurtResult == nullptr)return;
	//客户端先行，先进入到受伤动画
	if (MotionMachine->ChangeState((int32)EMotionMachineState::Hurt, (int32)EMotionMachineTransition::ToHurt))
	{
		ShowOrHideWeapon(false);
		if (Skill)
		{
			Skill->CurSkillFinished();
		}
		LastHurtResult = HurtResult;
		//从NPC表中得到受击动画
		//TODO
		//暂时写死
		if (CharacterInfo)
		{
			StopAnimMontage(CurHurtMontage);
			FString Path;
			if (CharacterInfo->GetCharacterType() == ECharaterType::Monster)
			{
				Path = "/Game/Character/Enemy/Gobin/Anims/Montage/Hurt";
			}
			else
			{
				Path = "/Game/Character/Hero/Anim/Montage/Hurt";
			}
			FOnMontageEnded EndDelagate;
			EndDelagate.BindUObject(this, &ACSCharacter::OnPlayHurtFinished);
			PlayMontage(Path, CurHurtMontage, EndDelagate);
		}

	}
}

void ACSCharacter::ToDead(ACSCharacter* Attacker)
{
	if (MotionMachine->ChangeState((int32)EMotionMachineState::Dead, (int32)EMotionMachineTransition::ToDead))
	{
		//打断技能
		if (Skill)
		{
			Skill->CurSkillFinished();
		}
	}
}

bool ACSCharacter::ToSkill(FString ComboPath)
{
	if (MotionMachine->ChangeState((int32)EMotionMachineState::Skill, (int32)EMotionMachineTransition::ToSkill))
	{
		ShowOrHideWeapon(true);
		HasAtkEnemys.Empty();
		ActiveWeaponTrigger(false);
		StopMove();
		//播放前的一些准备动作
		StopAnimMontage(CurComMontage);
		PrepareComboClip();
		FOnMontageEnded EndDelagate;
		EndDelagate.BindUObject(this, &ACSCharacter::OnComboEnded);
		PlayMontage(ComboPath, CurComMontage, EndDelagate);
		return true;
	}
	else
	{
		if (Skill)
		{
			Skill->CurSkillFinished();
		}
		return false;
	}

}

void ACSCharacter::ForcibleToIdle()
{
	StopMove();
	MotionMachine->ForcibleChangeState((int32)EMotionMachineState::Idle);
}

bool ACSCharacter::CanReleseSkill()
{
	bool IsOk = MotionMachine->CanChange((int32)EMotionMachineState::Skill, (int32)EMotionMachineTransition::ToSkill);
	return IsOk;
}

void ACSCharacter::OnComboEnded(UAnimMontage* Montage, bool bInterrupted)
{
	if (!bInterrupted && CurComMontage == Montage && MotionMachine->GetCurrentStateID() == (int32)EMotionMachineState::Skill)
	{
		ShowOrHideWeapon(false);
		//回到Idle
		ToIdle();
		if (Skill)
		{
			Skill->CurSkillFinished();
			StopMove();
		}
	}
}
void ACSCharacter::OnPlayHurtFinished(UAnimMontage* Montage, bool bInterrupted)
{
	if (!bInterrupted && CurHurtMontage == Montage && MotionMachine->GetCurrentStateID() == (int32)EMotionMachineState::Hurt)
	{
		ToIdle();
	}
}

void ACSCharacter::OnPlayDeadFinished(UAnimMontage* Montage, bool bInterrupted)
{

}

void ACSCharacter::PrepareComboClip()
{

}

void ACSCharacter::ActiveWeaponTrigger(bool bActive)
{
	if (CharacterInfo->GetCharacterType() == ECharaterType::MainPlayer && Weapon)
	{
		if (bActive)
		{
			Weapon->ActiveBox();
		}
		else
		{
			Weapon->DeactiveBox();
		}
	}
}

void ACSCharacter::ShowOrHideWeapon(bool bShow)
{
	if (Weapon)
	{
		if (bShow)
		{
			Weapon->Show();
		}
		else
		{
			Weapon->Hide();
		}
	}
}

void ACSCharacter::AtkEnemy(ACSCharacter* Enemy)
{
	if (CharacterInfo->GetCharacterType() == ECharaterType::MainPlayer && Enemy && !HasAtkEnemys.Contains(Enemy))
	{
		UCSComboClip* ComboClip = Skill->GetCurSkill()->GetCurComboClip();
		UCSHurtResult* HurtResult = NewObject<UCSHurtResult>();
		HurtResult->Init(this, Enemy, 0, ComboClip->GetId());
		Enemy->ToHurt(HurtResult);
		HasAtkEnemys.Add(Enemy);
		//发送攻击请求
		fightV2::FightRequest FightRequest;
		FightRequest.set_skillid(ComboClip->GetId());
		FightRequest.set_targetid(Enemy->GetCharacterInfo()->GetID());
		FightRequest.set_x(GetActorLocation().X);
		FightRequest.set_y(GetActorLocation().Y);
		FightRequest.set_z(GetActorLocation().Z);
		g_pGameInstance->SendMessage(69001, &FightRequest);
	}

}

void ACSCharacter::SetWeapon(ACSWeapon* InWeapon)
{
	Weapon = InWeapon;
}

ACSWeapon* ACSCharacter::GetWeapon()
{
	return Weapon;
}
