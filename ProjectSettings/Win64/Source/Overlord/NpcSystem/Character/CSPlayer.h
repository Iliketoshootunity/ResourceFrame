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
	// 输入操作相关
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;		//绑定输入
	void MoveForward(float Val);																		//向前后移动
	void MoveRight(float Val);																			//左右移动					

	UFUNCTION(BlueprintCallable, Category = Input) void NormalAttackPressed();
	UFUNCTION(BlueprintCallable, Category = Input) void NormalAttackRelesed();
	UFUNCTION(BlueprintCallable, Category = Input) void Skill1Pressed();
	UFUNCTION(BlueprintCallable, Category = Input) void Skill1Relesed();
	UFUNCTION(BlueprintCallable, Category = Input) void OnRotateToTagrget(float RoateSpeed, float MaxAngle, float RotateTime = 0);
	void CoverOperation(float NewRockerX, float NewRockerY, float NewSourceYaw, float NewControllerYaw, float NewTargetYaw);	//*覆盖操作信息*/
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
	// 状态机相关
protected:
	virtual void OnComboEnded(UAnimMontage* Montage, bool bInterrupted) override;
	virtual void PrepareComboClip() override;
	//////////////////////////////////////////////////////////////////////////
	// 消息
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
