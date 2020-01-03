// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "CSHead.generated.h"

class FMessage;
class UWidgetComponent;
class UUIHead;
class UCSGameCharacter;
/**
 *ͷ��UI�߼�����
 */
UCLASS()
class OVERLORD_API UCSHead : public UObject
{
	GENERATED_BODY()
public:
	UCSHead();
	~UCSHead();
public:
	virtual void Init(UCSGameCharacter* InBaseCharacter);
	//////////////////////////////////////////////////////////////////////////
// Message
protected:
	/*bind MessageDispatcher message*/
	virtual void BindMessage();
	/*unbind MessageDispatcher message*/
	virtual void UnBindMessage();
protected:
	virtual void OnHpChange(FMessage* Message);
	virtual void OnMaxHpChange(FMessage* Message);
	virtual void OnSetCharateName(FMessage* Message);
protected:
	UPROPERTY()
	UWidgetComponent* HeadWidgetComponent;
	UPROPERTY()
	UUIHead* HeadWidget;
	UPROPERTY()
	UCSGameCharacter* BaseCharacter;

	bool bActorUnitInstance;
public:
	virtual FORCEINLINE UUIHead* GetHeadWidget() { return HeadWidget; };
};