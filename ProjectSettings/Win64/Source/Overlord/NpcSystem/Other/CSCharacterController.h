// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "Protoc/move.pb.h"
#include "Protoc/test.pb.h"
#include "CSCharacterController.generated.h"


class ACSCharacter;
/**
 *
 */
UCLASS()
class OVERLORD_API ACSCharacterController : public APlayerController
{
	GENERATED_BODY()
public:
	//virtual void BeginPlay() override;
	// Called every frame
	virtual void Tick(float DeltaTime) override;
//	//////////////////////////////////////////////////////////////////////////
//	// Message
//public:
//	void SendMoveMessage(float DeltaTime);
//	void RespondMoveMessage(moveV2::ResPlayerWalk WalkMessage);
//protected:
//	FVector OldMoveMessagePosition;
//	float MoveMessageTimer;
//
//public:
//	void SendComboMessage(int32 ComboId);
//	void RespondComboMessage(test::ComboNode ComboNode);
//	void SendComboEndMessage();
//	void RespondComboEndMessage();
//
//public:
//	void SendRotateYawMessage(float Yaw, float Time);
//	void RespondRotateYawMessage(int32 Yaw, int32 Time);
//	void HandleRotateYawMessage();
//protected:
//	float CacheYaw_RotateYawMessage;
//	float CacheTime_RotateYawMessage;
//	float HandleRotateYawMessageTime;
//	float RespondRotateYawMessageTime;
//	bool bHandleRotateYawMessageFlag;



protected:
	UPROPERTY()
	ACSCharacter* ControlledCharacter;
};
