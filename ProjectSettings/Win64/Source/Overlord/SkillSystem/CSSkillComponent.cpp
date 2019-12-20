// Fill out your copyright notice in the Description page of Project Settings.


#include "CSSkillComponent.h"
#include "GameFramework/Character.h"
#include "BasicFunction.h"
#include "CSSkill.h"
#include "Components/SkeletalMeshComponent.h"
#include "SkillTable.h"
#include "CSComboClip.h"
#include "NpcSystem/Character/CSCharacter.h"

// Sets default values for this component's properties
UCSSkillComponent::UCSSkillComponent()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = true;
	// ...
}

// Called when the game starts
void UCSSkillComponent::BeginPlay()
{
	Super::BeginPlay();
	// ...

	Init();
}


void UCSSkillComponent::Init()
{
	ACSCharacter* Character = Cast<ACSCharacter>(this->GetOwner());
	if (Character)
	{
		OwnerCharacter = Character;
		TMap<int32, FSkillTableData>* SkillMap = SkillTable::Get()->GetData();
		for (TMap<int32, FSkillTableData>::TIterator It(*SkillMap); It; ++It)
		{
			UCSSkill* NewSkill = NewObject<UCSSkill>(this);
			if (NewSkill)
			{
				NewSkill->Init(this, &It.Value());
				SkillArray.Add(NewSkill);
			}
			else
			{
				FBasicFunction::Log("USkillComponent::Init() USkill  is Null", true);
				return;
			}

		}
	}
}


void UCSSkillComponent::TickComponent(float DeltaTime, enum ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	SkillTick();
}

void UCSSkillComponent::SkillTick()
{
	if (bSkillInput)
	{
		RespondInput(SkillButtonType, EInputStatus::Persistent, InputSkillId);
	}
}

void UCSSkillComponent::RespondInput(ESkillButtonType ButtonType, EInputStatus InputStatus, int32 SkillID)
{
	if (!OwnerCharacter->CanReleseSkill())return;
	if (CurSkill == nullptr)
	{
		//第一个技能只有按下状态才会触发
		if (InputStatus == EInputStatus::Press)
		{
			int state = 0;//地面状态
			if (OwnerCharacter->GetCharacterMovement()->IsFalling())
			{
				//空中状态
				state = 1;
			}
			UCSSkill* Skill = nullptr;
			for (size_t i = 0; i < SkillArray.Num(); i++)
			{
				if (SkillID == SkillArray[i]->GetSkillData()->SkillID)
				{
					Skill = SkillArray[i];
				}
				//if (SkillArray[i]->GetSKillTriggerType() == state)
				//{
				//	Skill = SkillArray[i];
				//}
			}
			if (Skill)
			{
				Skill->Play();
				CurSkill = Skill;
			}
			else
			{
				return;
			}
		}
	}
	else
	{
		if (CurSkill && CurSkill->GetID() == SkillID)
		{
			if (CurSkill->GetCurComboClip())
			{
				CurSkill->GetCurComboClip()->RespondInput(InputStatus);
			}
			else
			{
				return;
			}
		}
		else
		{
			return;
		}
	}
	InputSkillId = SkillID;
	SkillButtonType = ButtonType;
	if (InputStatus == EInputStatus::Press)
	{
		bSkillInput = true;
	}
	else if (InputStatus == EInputStatus::Relese)
	{
		bSkillInput = false;
	}

}

void UCSSkillComponent::RespondConstructComboJumpLine(FComboJumpLineData Data)
{
	if (CurSkill)
	{
		UCSComboClip* Clip = CurSkill->GetCurComboClip();
		if (Clip)
		{
			CurSkill->GetCurComboClip()->RespondConstructComboJumpLine(Data);
		}
	}
	int32 ID = Data.ID;
	FBasicFunction::Log(FString::FromInt(ID), true);
}

void UCSSkillComponent::RespondDeactiveComboJumpLine(int32 Index)
{
	if (CurSkill)
	{
		if (CurSkill->GetCurComboClip())
		{
			CurSkill->GetCurComboClip()->RespondDeactiveComboJumpLine(Index);
		}
	}
}

void UCSSkillComponent::RespondJumpOther(int32 Index)
{
	if (CurSkill)
	{
		if (CurSkill->GetCurComboClip())
		{
			CurSkill->GetCurComboClip()->RespondJumpOther(Index);
		}
	}
}

void UCSSkillComponent::RespondComboMessage(int32 ComboId)
{
	if (CurSkill == nullptr)
	{
		for (size_t i = 0; i < SkillArray.Num(); i++)
		{
			const FSkillTableData* SkillData = SkillArray[i]->GetSkillData();
			if (SkillData)
			{
				if (SkillData->FirstComboClipId == ComboId)
				{
					CurSkill = SkillArray[i];
					break;
				}
			}
			else
			{
				FBasicFunction::Log("USkillComponent::RespondComboMessage  SkillData is Null");
			}
		}
		if (CurSkill == nullptr)
		{
			FBasicFunction::Log("USkillComponent::RespondComboMessage  CurSkill is Null");
		}

	}
	if (CurSkill)
	{
		CurSkill->RespondComboMessage(ComboId);
	}
}

void UCSSkillComponent::CurSkillFinished()
{
	//数据清空
	if (CurSkill)
	{
		CurSkill->Reset();
		CurSkill = nullptr;
	}

}

void UCSSkillComponent::Destroy()
{
	if (SkillArray.Num() > 0)
	{
		for (size_t i = 0; i < SkillArray.Num(); i++)
		{
			SkillArray[i]->Destroy();
		}
	}
	SkillArray.Empty();
	CurSkill = nullptr;
	OwnerCharacter = nullptr;
}


