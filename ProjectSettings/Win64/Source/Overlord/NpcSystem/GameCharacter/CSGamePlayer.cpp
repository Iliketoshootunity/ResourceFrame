// Fill out your copyright notice in the Description page of Project Settings.


#include "CSGamePlayer.h"
#include "NpcResTable.h"
#include "Protoc/map.pb.h"

UCSGamePlayer::UCSGamePlayer()
{

}

void UCSGamePlayer::Init(void* Info)
{
	CharacterType = ECharaterType::Player;
	mapV2::RoundPlayerInfo* PlayerInfo = (mapV2::RoundPlayerInfo*)Info;
	if (PlayerInfo)
	{
		ID = PlayerInfo->rid();
		SetMaxHP(PlayerInfo->maxhp());
		SetHP(PlayerInfo->hp());
		std::string NameStr = PlayerInfo->name();
		FString NameTemp = FString(NameStr.c_str());
		SetCharacterName(NameTemp);

		int32 TableId = 3;
		const FNpcsTableData* Data = NpcsTable::Get()->GetData(TableId);
		CharacterTableData = Data;
		if (Data)
		{
			const FRoleTypeData* UnitTypeData = NpcResTable::Get()->GetRoleTypeData(CharacterTableData->RoleTypeID);
			CharacterTypeData = UnitTypeData;
		}

		InitPos = FVector(PlayerInfo->x(), PlayerInfo->y(), PlayerInfo->z());
		InitRot = FRotator::ZeroRotator;
	}
}
