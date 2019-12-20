// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "CSComboJumpLine.h"
#include "CSSkillDefine.h"
#include "ComboClipTable.h"
#include "CSComboClip.generated.h"

class UCSSkill;
class UCSComboClip;
struct FComboJumpLineData;
typedef struct tagComboClipTableInfo FComboClipsTableData;



DECLARE_DELEGATE_ThreeParams(FCreateAndPlayComboClipDelegate, FComboClipsTableData*, FString, FComboJumpLineData)
DECLARE_DELEGATE(FChargeInterruptedDelegate)
/**
 *
 */
UCLASS(Blueprintable)
class OVERLORD_API UCSComboClip : public UObject
{
	GENERATED_BODY()
private:

	FCreateAndPlayComboClipDelegate CreateAndPlayComboClipDelegate;
	FChargeInterruptedDelegate ChargeInterruptedDelegate;

	FString JumpMontage;
	const FComboClipsTableData* ClipData;
	FComboJumpLineData PreLineData;

	//������ת��JumpLine
	UPROPERTY() TArray<UCSComboJumpLine*> JumpLines;
	//������ת��JumpLine������
	UPROPERTY() TArray<UCSComboJumpLine*>	CanJumpLines;
	//���ھ������ȿ�����תJumpLine�������ǵ�ǰCanJumpLineBaseAbsolute���ᴦ��CanJumpLineBasePriority
	UPROPERTY() UCSComboJumpLine* CanJumpLineBaseAbsolute;
	//�������ȼ��Ŀ�����תJumpLine
	UPROPERTY() UCSComboJumpLine* CanJumpLineBasePriority;

public:
	FORCEINLINE int32 GetId() { return ClipData->ID; }
	FORCEINLINE FString GetJumpMontage() { return JumpMontage; }
	FORCEINLINE FComboClipsTableData* GetClipData() { return (FComboClipsTableData*)ClipData; }
	FORCEINLINE FComboJumpLineData GetPreLineData() { return PreLineData; }

public:
	//��ʼ��
	void Init(const FComboClipsTableData* ClipData, FString JumpMontageName, FComboJumpLineData LineData);	
	//��Ӧ����
	void RespondInput(EInputStatus InputStatus);
	//��Ӧ����ComboJumpLine
	void RespondConstructComboJumpLine(FComboJumpLineData NewLineData);
	//��Ӧ������ComboJumpLine
	void RespondDeactiveComboJumpLine(int32 Index);
	//��Ӧ��ת���¸���̫��
	void RespondJumpOther(int32 Index);
	//ƥ������ɹ�����������޳�һЩ���ȼ��͵���ˮ��
	void MatchInputSucceed(UCSComboJumpLine* JumpLine);
	//ƥ������ʧ��
	void MatchInputFailed(UCSComboJumpLine* JumpLine);
	//�Ƿ���JumpLines
	bool IsContainJumpLines(UCSComboJumpLine* JumpLine);
	//���ô��������Żص�
	void SetCreateAndPlayComboClipDelegate(FCreateAndPlayComboClipDelegate NewCreateAndPlayComboClipDelegate);
	//���ô�������ص�
	void SetChargeInterruptedDelegate(FChargeInterruptedDelegate NewChargeInterruptedDelegate);
	//��������
	void Reset();
};
