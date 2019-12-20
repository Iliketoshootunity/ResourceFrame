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
		//构建时即匹配成功
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
			//这个类型只有抬起时才算失败
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
		//优先处理绝对优先的可跳转片段
		if (Index == CanJumpLineBaseAbsolute->GetIndex())
		{
			TempLine = CanJumpLineBaseAbsolute;
		}
	}
	else if (CanJumpLineBasePriority)
	{
		//匹配成功
		if (Index == CanJumpLineBasePriority->GetIndex())
		{
			TempLine = CanJumpLineBasePriority;
		}
	}
	//播放下一个Combo
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
		//需要对这种类型的做特殊处理
		CanJumpLineBaseAbsolute = JumpLine;
		//不再响应输入
		CanJumpLines.Remove(CanJumpLineBaseAbsolute);
		return;
	}
	else
	{
		//比较当前的优先级
		if (CanJumpLineBasePriority != nullptr && CanJumpLineBasePriority->GetPriority() < JumpLine->GetPriority())
		{
			MatchInputFailed(JumpLine);
			return;
		}
		//剔除比当前优先级低的选项
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
			//匹配失败
			MatchInputFailed(TempArr[i]);
		}
		//替换已经触发的且优先级最高的JumpLine
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
			//打断蓄力动作,回到Idle状态，技能使用状态回归与无
			ChargeInterruptedDelegate.ExecuteIfBound();
		}
	}
	else if (JumpLine->GetRespondType() == EComboJumLineRespondType::BeforeCharge)
	{
		//优先处理的跳转已经没了，可以处理其他根据优先级来寻找需要的跳转了
		if (JumpLine == CanJumpLineBaseAbsolute)
		{
			CanJumpLineBaseAbsolute = nullptr;
			//不再响应输入
			CanJumpLines.Remove(JumpLine);
		}
		else
		{
			FBasicFunction::Log("Erroc!!Do not config two  CanJumpLineBaseAbsolute", true, 20);
		}
	}
	else
	{
		//只是删除当前跳转，其他跳转还可以响应输入
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

