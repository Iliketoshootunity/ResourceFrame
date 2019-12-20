// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "CSHurtResult.generated.h"

class ACSCharacter;
typedef struct tagComboClipTableInfo FComboClipsTableData;
/**
 *
 */
UCLASS()
class OVERLORD_API UCSHurtResult : public UObject
{
	GENERATED_BODY()
public:
	UCSHurtResult();
protected:
	ACSCharacter* Attacker;
	ACSCharacter* BeAttacker;
	int32 SkillId;
	int32 ComboId;
	const FComboClipsTableData* ClipData;

	bool bNeedAdjustPosition;				//�Ƿ���Ҫ����λ��,��Щ������Ҫ�����ߺͱ������߱���һ������
	FVector AdjustPosition;
public:
	void Init(ACSCharacter* InAttacker, ACSCharacter* InBeAttacker, int32 InSkillId, int32 InComboId);
};
