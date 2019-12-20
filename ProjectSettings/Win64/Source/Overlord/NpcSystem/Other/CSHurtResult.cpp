// Fill out your copyright notice in the Description page of Project Settings.


#include "CSHurtResult.h"
#include "Kismet/KismetMathLibrary.h"
#include "../Character/CSCharacter.h"
#include "ComboClipTable.h"

UCSHurtResult::UCSHurtResult()
{

}

void UCSHurtResult::Init(ACSCharacter* InAttacker, ACSCharacter* InBeAttacker, int32 InSkillId, int32 InComboId)
{
	Attacker = InAttacker;
	BeAttacker = InBeAttacker;
	SkillId = InSkillId;
	ComboId = InComboId;
	ClipData = ComboClipTable::Get()->GetData(ComboId);
	//���ԣ���ʽ����
	if (ComboId == 10004)
	{
		bNeedAdjustPosition = true;
	}
	if (bNeedAdjustPosition)
	{
		float KeepDistance = 300;		//���ԣ���ʽ����
		FVector AttackerPos = Attacker->GetActorLocation();
		FVector BeAttackerPos = BeAttacker->GetActorLocation();
		float Distance = UKismetMathLibrary::Vector_Distance(FVector(AttackerPos.X, AttackerPos.Y, 0), FVector(BeAttackerPos.X, BeAttackerPos.Y, 0));
		if (Distance < KeepDistance)
		{
			FVector RelativePos = BeAttackerPos - AttackerPos;
			RelativePos = FVector(RelativePos.X, RelativePos.Y, 0);
			RelativePos = UKismetMathLibrary::Normal(RelativePos);
			AdjustPosition = RelativePos * KeepDistance + BeAttackerPos;
			//���߼��Ŀ��λ����û���赲
			//TODO
			BeAttacker->SetActorLocation(AdjustPosition);
		}
	}

	FRotator Rot = UKismetMathLibrary::FindLookAtRotation(BeAttacker->GetActorLocation(), Attacker->GetActorLocation());
	BeAttacker->SetActorRotation(Rot);
}
