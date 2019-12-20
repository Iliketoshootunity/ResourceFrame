// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "CSSkillDefine.h"
#include "CSSkillData.generated.h"


USTRUCT(BlueprintType)
struct FComboJumpLineData
{
	GENERATED_BODY()

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite) int32 ID;
	UPROPERTY(EditAnywhere, BlueprintReadWrite)	int32 Index;
	UPROPERTY(EditAnywhere, BlueprintReadWrite)	int32 Priority;
	UPROPERTY(EditAnywhere, BlueprintReadWrite)	FString JumpMontageName;
	UPROPERTY(EditAnywhere, BlueprintReadWrite)	EComboJumLineRespondType RespondType;

	//≤‚ ‘ ˝æ›
	//UPROPERTY(EditAnywhere, BlueprintReadWrite) bool IsAcceptMoveInput;
};
