// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "FSMState.generated.h"

/**
 *
 */
UCLASS()
class OVERLORD_API UFSMState : public UObject
{
	GENERATED_BODY()

protected:
	/*Transition Map*/
	TMap<int, int> TransitionMap;
	int32 StateId;
public:
	FORCEINLINE int32 GetStateId() { return StateId; };

public:
	/*Add Transition*/
	void AddTransition(int32 Transition, int32 StateId);
	/*Delete Transition*/
	void DeleteTransition(int32 Transition);
	/*Ge tOut Put State*/
	int32 GetOutPutState(int32 Transition);
public:

	virtual void Init(int32 StateId);

	/*Enter State*/
	virtual void Enter();

	/*Update State*/
	virtual void Update();

	/*State Exit*/
	virtual void Exit();

};
