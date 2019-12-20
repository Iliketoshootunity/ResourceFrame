// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Protoc/fight.pb.h"
#include "Protoc/move.pb.h"
#include "CSCharacter.h"
#include "CSPlayer.generated.h"

/**
 *
 */
UCLASS()
class OVERLORD_API ACSPlayer : public ACSCharacter
{
	GENERATED_BODY()
public:
	ACSPlayer();

public:
	//////////////////////////////////////////////////////////////////////////
	// ����������
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;		//������
	void MoveForward(float Val);																		//��ǰ���ƶ�
	void MoveRight(float Val);																			//�����ƶ�					

	UFUNCTION(BlueprintCallable, Category = Input) void NormalAttackPressed();
	UFUNCTION(BlueprintCallable, Category = Input) void NormalAttackRelesed();
	UFUNCTION(BlueprintCallable, Category = Input) void Skill1Pressed();
	UFUNCTION(BlueprintCallable, Category = Input) void Skill1Relesed();
	UFUNCTION(BlueprintCallable, Category = Input) void OnRotateToTagrget(float RoateSpeed, float MaxAngle, float RotateTime = 0);
	void CoverOperation(float NewRockerX, float NewRockerY, float NewSourceYaw, float NewControllerYaw, float NewTargetYaw);	//*���ǲ�����Ϣ*/
public:
	UFUNCTION(BlueprintPure, Category = Input) FORCEINLINE float GetRockerInputX() { return RockerX; };
	UFUNCTION(BlueprintPure, Category = Input) FORCEINLINE float GetRockerInputY() { return RockerY; };
protected:
	float RockerX;
	float RockerY;
	float RockerSize;

	float CacheSourceYaw;
	float CacheTargetYaw;
	float CacheRockerX;
	float CacheRockerY;
	float CacheControllerYaw;
	//////////////////////////////////////////////////////////////////////////
	// ״̬�����
protected:
	virtual void OnComboEnded(UAnimMontage* Montage, bool bInterrupted) override;
	virtual void PrepareComboClip() override;
	//////////////////////////////////////////////////////////////////////////
	// ��Ϣ
protected:
	void MoveMessage(FVector Target, float MaxWalkSpeed);
	UFUNCTION(BlueprintImplementableEvent, Category = MoveMessage) FVector CalMoveMessageTarget();

protected:
	float CacheMaxSpeed;
	FVector OldMoveMessagePosition;
	float MoveMessageTimer;
public:
	void SendMoveMessage(float DeltaTime);
	void SendComboMessage(int32 ComboId);
	void SendComboEndMessage();
};
