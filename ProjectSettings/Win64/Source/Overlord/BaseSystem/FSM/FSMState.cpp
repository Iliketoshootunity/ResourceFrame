// Fill out your copyright notice in the Description page of Project Settings.


#include "FSMState.h"

void UFSMState::AddTransition(int32 Transition, int32 StateId)
{
	if (TransitionMap.Contains(Transition))return;
	TransitionMap.Add(Transition, StateId);
}

void UFSMState::DeleteTransition(int32 Transition)
{
	if (TransitionMap.Contains(Transition))
	{
		TransitionMap.Remove(Transition);
	}
}

int32 UFSMState::GetOutPutState(int32 Transition)
{
	if (TransitionMap.Contains(Transition))
	{
		return TransitionMap[Transition];
	}
	return -1;
}

void UFSMState::Init(int32 StateId)
{
	this->StateId = StateId;
}

void UFSMState::Enter()
{

}

void UFSMState::Update()
{

}

void UFSMState::Exit()
{

}
