// Fill out your copyright notice in the Description page of Project Settings.


#include "CSComboClip.h"
#include "BasicFunction.h"
#include "CSSkill.h"
#include "CSComboJumpLine.h"
#include "CSSkillData.h"
#include "Kismet/KismetSystemLibrary.h"
#include "Engine/EngineTypes.h"


void UCSComboClip::Init(const FComboClipsTableData* ClipData, FString JumpMontageName, FComboJumpLineData LineData)
{
	this->ClipData = ClipData;
	this->JumpMontage = JumpMontageName;
	this->PreLineData = LineData;
}

void UCSComboClip::RespondInput(EInputStatus InputStatus)
{
	if (CanJumpLineBaseAbsolute)
	{
		CanJumpLineBaseAbsolute->RespondInput(InputStatus);
	}
	for (size_t i = 0; i < CanJumpLines.Num(); i++)
	{
		UCSComboJumpLine* Temp = CanJumpLines[i];
		Temp->RespondInput(InputStatus);
	}
}

void UCSComboClip::RespondConstructComboJumpLine(FComboJumpLineData NewLineData)
{
	for (size_t i = 0; i < JumpLines.Num(); i++)
	{
		if (JumpLines[i]->GetIndex() == NewLineData.Index)
		{
			return;
		}
	}
	UCSComboJumpLine* NewJumpLine = NewObject<UCSComboJumpLine>(this);
	NewJumpLine->Init(this, NewLineData);
	if (NewLineData.RespondType == EComboJumLineRespondType::BeforeCharge)
	{
		if (CanJumpLineBaseAbsolute != nullptr)
		{
			FBasicFunction::Log("Erroc!!Do not config two  CanJumpLineBaseAbsolute", true, 20);
		}
		//����ʱ��ƥ��ɹ�
		MatchInputSucceed(NewJumpLine);
	}
	CanJumpLines.Add(NewJumpLine);
	JumpLines.Add(NewJumpLine);
}

void UCSComboClip::RespondDeactiveComboJumpLine(int32 Index)
{
	for (size_t i = 0; i < CanJumpLines.Num(); i++)
	{
		if (CanJumpLines[i]->GetIndex() == Index)
		{
			//�������ֻ��̧��ʱ����ʧ��
			if (CanJumpLines[i]->GetRespondType() == EComboJumLineRespondType::BeforeCharge)
			{
				continue;
			}
			MatchInputFailed(CanJumpLines[i]);
		}
	}
}

void UCSComboClip::RespondJumpOther(int32 Index)
{
	UCSComboJumpLine* TempLine = nullptr;
	if (CanJumpLineBaseAbsolute)
	{
		//���ȴ���������ȵĿ���תƬ��
		if (Index == CanJumpLineBaseAbsolute->GetIndex())
		{
			TempLine = CanJumpLineBaseAbsolute;
		}
	}
	else if (CanJumpLineBasePriority)
	{
		//ƥ��ɹ�
		if (Index == CanJumpLineBasePriority->GetIndex())
		{
			TempLine = CanJumpLineBasePriority;
		}
	}
	//������һ��Combo
	if (TempLine && TempLine->GetCanJump())
	{
		CreateAndPlayComboClipDelegate.ExecuteIfBound((FComboClipTableData*)TempLine->GetComboClipsTableData(),
			TempLine->GetMontageName(), TempLine->GetComboJumpLineData());
	}
}

void UCSComboClip::MatchInputSucceed(UCSComboJumpLine* JumpLine)
{
	if (JumpLine == nullptr)return;
	if (JumpLine->GetRespondType() == EComboJumLineRespondType::BeforeCharge)
	{
		//��Ҫ���������͵������⴦��
		CanJumpLineBaseAbsolute = JumpLine;
		//������Ӧ����
		CanJumpLines.Remove(CanJumpLineBaseAbsolute);
		return;
	}
	else
	{
		//�Ƚϵ�ǰ�����ȼ�
		if (CanJumpLineBasePriority != nullptr && CanJumpLineBasePriority->GetPriority() < JumpLine->GetPriority())
		{
			MatchInputFailed(JumpLine);
			return;
		}
		//�޳��ȵ�ǰ���ȼ��͵�ѡ��
		TArray<UCSComboJumpLine*> TempArr;
		for (size_t i = 0; i < CanJumpLines.Num(); i++)
		{
			int32 TempPriority = CanJumpLines[i]->GetPriority();
			if (JumpLine->GetPriority() < TempPriority)
			{
				TempArr.Add(CanJumpLines[i]);
			}
		}
		for (size_t i = 0; i < TempArr.Num(); i++)
		{
			//ƥ��ʧ��
			MatchInputFailed(TempArr[i]);
		}
		//�滻�Ѿ������������ȼ���ߵ�JumpLine
		CanJumpLineBasePriority = JumpLine;
	}

}

void UCSComboClip::MatchInputFailed(UCSComboJumpLine* JumpLine)
{
	if (JumpLine == nullptr)return;
	if (JumpLine->GetRespondType() == EComboJumLineRespondType::Charge)
	{
		if (CanJumpLineBasePriority == nullptr)
		{
			//�����������,�ص�Idle״̬������ʹ��״̬�ع�����
			ChargeInterruptedDelegate.ExecuteIfBound();
		}
	}
	else if (JumpLine->GetRespondType() == EComboJumLineRespondType::BeforeCharge)
	{
		//���ȴ������ת�Ѿ�û�ˣ����Դ��������������ȼ���Ѱ����Ҫ����ת��
		if (JumpLine == CanJumpLineBaseAbsolute)
		{
			CanJumpLineBaseAbsolute = nullptr;
			//������Ӧ����
			CanJumpLines.Remove(JumpLine);
		}
		else
		{
			FBasicFunction::Log("Erroc!!Do not config two  CanJumpLineBaseAbsolute", true, 20);
		}
	}
	else
	{
		//ֻ��ɾ����ǰ��ת��������ת��������Ӧ����
		CanJumpLines.Remove(JumpLine);
	}
}

bool UCSComboClip::IsContainJumpLines(UCSComboJumpLine* JumpLine)
{
	if (JumpLine == nullptr)return false;
	return JumpLines.Contains(JumpLine);
}

void UCSComboClip::SetCreateAndPlayComboClipDelegate(FCreateAndPlayComboClipDelegate NewCreateAndPlayComboClipDelegate)
{
	CreateAndPlayComboClipDelegate = NewCreateAndPlayComboClipDelegate;
}

void UCSComboClip::SetChargeInterruptedDelegate(FChargeInterruptedDelegate NewChargeInterruptedDelegate)
{
	ChargeInterruptedDelegate = NewChargeInterruptedDelegate;
}

void UCSComboClip::Reset()
{
	for (size_t i = 0; i < JumpLines.Num(); i++)
	{
		JumpLines[i]->Reset();
	}
	JumpLines.Empty();
	CanJumpLines.Empty();
	CanJumpLineBasePriority = nullptr;
	CanJumpLineBaseAbsolute = nullptr;
	CreateAndPlayComboClipDelegate = nullptr;
	ChargeInterruptedDelegate = nullptr;
}

