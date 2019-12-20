// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "FSMMachine.generated.h"

class UFSMState;

/**
 *
 */
UCLASS()
class OVERLORD_API UFSMMachine : public UObject
{
	GENERATED_BODY()
protected:
	UPROPERTY()
		TArray<UFSMState*> FSMStates;

	int32 CurrentStateID;
	UPROPERTY()
		UFSMState* CurrentState;


public:
	FORCEINLINE int32 GetCurrentStateID() { return CurrentStateID; }
	FORCEINLINE UFSMState* GetCurrentState() { return CurrentState; }


public:
	void AddState(UFSMState* State);

	void DeleteState(UFSMState* State);

	bool ChangeState(int32 ToStateID,int32 Transition);

	void ForcibleChangeState(int32 ToStateID);

	bool CanChange(int32 ToStateID,int32 Transition);
};
