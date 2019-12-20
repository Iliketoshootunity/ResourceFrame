// Fill out your copyright notice in the Description page of Project Settings.

#include "SkillTable.h"



SkillTable::SkillTable() :STabBaseTable(this)
{

}

SkillTable::~SkillTable()
{

}

bool SkillTable::ReadTable(int32 nRow, int32& nCol)
{
	FSkillTableData data;

	nCol++;

	if (!GetTableManager()->GetTabInteger(nRow, nCol++, INVALID_INDEX, data.SkillID))
		return false;
	if (!GetTableManager()->GetTabString(nRow, nCol++, ANSI_NONE, data.SkillName))
		return false;
	if (!GetTableManager()->GetTabInteger(nRow, nCol++, INVALID_INDEX, data.TriggerType))
		return false;
	if (!GetTableManager()->GetTabString(nRow, nCol++, ANSI_NONE, data.FirstComboMontage))
		return false;
	if (!GetTableManager()->GetTabInteger(nRow, nCol++, INVALID_INDEX, data.SkillNeedRace))
		return false;
	if (!GetTableManager()->GetTabInteger(nRow, nCol++, INVALID_INDEX, data.SkillNeedWeapon))
		return false;
	if (!GetTableManager()->GetTabInteger(nRow, nCol++, INVALID_INDEX, data.FirstComboClipId))
		return false;

	SetData(data.SkillID, data);

	return true;
}