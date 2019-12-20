#pragma once

#pragma once
// Fill out your copyright notice in the Description page of Project Settings.

#include "CoreMinimal.h"
#include "BaseTable.h"

/************************************************************************
* Desc 	: ComboClip±í
* Author	: XiaoHailin
* Time		: [8/10/2019 xhl]
************************************************************************/

typedef struct tagComboClipTableInfo
{
	int32			ID;
	FString			Path;
	int32			ChantLength;
	int32			ChantTimeNum;
	int32			ChargeLength;
	int32			ChargeLengthMax;

}FComboClipTableData; 

class TABLEMODULE_API ComboClipTable : public STabBaseTable<ComboClipTable, FComboClipTableData>
{
public:
	ComboClipTable();
	virtual ~ComboClipTable();
public:
	virtual bool				ReadTable(int32 nRow, int32& nCol) override;
};
