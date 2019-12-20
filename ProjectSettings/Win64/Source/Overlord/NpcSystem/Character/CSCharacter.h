// Fill out your copyright notice in the Description page of Project Settings.
// Fill out your copyright notice in the Description page of Project Settings.
// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "SkillSystem/CSSkillDefine.h"
#include "Protoc/test.pb.h"
#include "BaseDefine.h"
#include "Components/SkeletalMeshComponent.h"
#include "Protoc/fight.pb.h"
#include "Animation/AnimInstance.h"
#include "CSCharacter.generated.h"


#define  MESSAGEFLOATMULTIPLE 1.f

class USkeletalMeshComponent;
class UCSSkillComponent;
class USpringArmComponent;
class UCSSearchEnemyComponent;
class UFSMMachine;
class UActorInterpMovementComponent;
class UCSHead;
class UWidgetComponent;
class UCSGameCharacter;
class ACSWeapon;
class UCSHurtResult;
/*
Motion Machine Transition
*/
UENUM(BlueprintType)
enum class EMotionMachineTransition :uint8
{
	None = 0,
	ToIdle,
	ToRun,
	ToSkill,
	ToRoll,
	ToHurt,
	ToDead
};


/*
*Motion Machine State
*/
UENUM(BlueprintType)
enum class EMotionMachineState :uint8
{
	None = 0,
	Idle,
	Run,
	Skill,
	Roll,
	Hurt,
	Dead
};


UCLASS()
class OVERLORD_API ACSCharacter : public ACharacter
{
	GENERATED_BODY()

public:
	// Sets default values for this character's properties
	ACSCharacter();

	//////////////////////////////////////////////////////////////////////////
	// Life Function
protected:
	virtual void BeginPlay() override;
public:
	virtual void BeginDestroy() override;
	virtual void Tick(float DeltaTime) override;
public:
	virtual void Init(UCSGameCharacter* InCharacterInfo);
	//////////////////////////////////////////////////////////////////////////
	// 自定义运动
public:
	void InterpMoveToTarget(FVector TargetRot, float Time);
	void StopMove();

	//////////////////////////////////////////////////////////////////////////
	// 动画
public:
	virtual float PlayAnimMontage(class UAnimMontage* AnimMontage, float InPlayRate /* = 1.f */, FName StartSectionName /* = NAME_None */) override;
	virtual void  PlayMontage(FString MontageName, UAnimMontage*& OutAnimMontag, FOnMontageEnded EndDelegate);
	virtual void  StopAnimMontage(class UAnimMontage* AnimMontage) override;
	virtual void  StopAllAnimMontages();
	int32 GetAnimationMode();
	//////////////////////////////////////////////////////////////////////////
	// 外观设置
public:
	USkeletalMeshComponent* GetPawnMesh() const;									//获取Mesh组件
	UFUNCTION(BlueprintCallable, Category = Mesh)
		virtual bool IsMainPlayer() const;											//是否是本地玩家
	bool IsAlive() const;															//是否存活
	void SetSkeletalMesh(USkeletalMesh* NewSkeletalMesh,							//设置模型
		FVector Scale3D,
		EAnimationMode::Type AnimMode,
		FString AnimInstanceName,
		TArray<FMaterialData> Materials,
		bool bReinitPose = true);
	//////////////////////////////////////////////////////////////////////////
	// 摄像机设置
public:
	UFUNCTION(BlueprintCallable, Category = Camera)
		void AdjustCameraBoomArmLength(float MinLength, float MaxLength, float Speed);
	virtual void MoveToTargetPosition(FVector TargetPostion, float MaxSpeed);	   //移动到目标点	

	//////////////////////////////////////////////////////////////////////////
	// 状态机
protected:
	virtual void ConstructionMotionMachine();										//构建状态机

protected:
	UPROPERTY()	UFSMMachine* MotionMachine;
public:
	virtual void ToIdle();
	virtual void ToHurt(UCSHurtResult* HurtResult);
	virtual void ToDead(ACSCharacter* Attacker);
	virtual bool ToSkill(FString ComboPath);
	virtual void ForcibleToIdle();													//强制回到Idle状态
	virtual bool CanReleseSkill();													//是否可以释放技能
protected:
	virtual void OnComboEnded(UAnimMontage* Montage, bool bInterrupted);			//技能动画动画结束回调
	virtual void OnPlayHurtFinished(UAnimMontage* Montage, bool bInterrupted);		//受伤动画结束回调
	virtual void OnPlayDeadFinished(UAnimMontage* Montage, bool bInterrupted);		//死亡动画结束回调

	virtual void PrepareComboClip();
protected:
	UPROPERTY()	UAnimMontage* CurHurtMontage;
	UPROPERTY()	UAnimMontage* CurDeadMontage;
	UPROPERTY()	UAnimMontage* CurComMontage;

	UPROPERTY() UCSHurtResult* LastHurtResult;
	//////////////////////////////////////////////////////////////////////////
	// 武器相关
public:
	UFUNCTION(BlueprintCallable, Category = Weapon) void ActiveWeaponTrigger(bool bActive);
	UFUNCTION(BlueprintCallable, Category = Weapon) void ShowOrHideWeapon(bool bShow);
	UFUNCTION(BlueprintCallable, Category = Weapon) virtual void AtkEnemy(ACSCharacter* Enemy);
	UPROPERTY()
	TArray<ACSCharacter*> HasAtkEnemys;
	UPROPERTY(EditAnywhere,Category = Weapon)
	ACSWeapon* Weapon;
	UFUNCTION(BlueprintCallable) void SetWeapon(ACSWeapon* InWeapon);
	UFUNCTION(BlueprintPure) ACSWeapon* GetWeapon();
	//////////////////////////////////////////////////////////////////////////
	// Component
protected:
	/** skill */
	UPROPERTY(VisibleDefaultsOnly, Category = Skill)	UCSSkillComponent* Skill;
	/** CameraBoom */
	UPROPERTY(VisibleDefaultsOnly, Category = Camera)	USpringArmComponent* CameraBoom;
	/** SearchEnemy */
	UPROPERTY(VisibleDefaultsOnly, Category = Search)	UCSSearchEnemyComponent* SearchEnemy;
	/** Actor Interp to Movement */
	UPROPERTY(VisibleDefaultsOnly, Category = Move)		UActorInterpMovementComponent* InterpMovement;
	/* Head:HP MP HurtTips.... */
	UPROPERTY(VisibleDefaultsOnly, Category = UI)		UWidgetComponent* HeadWidget;
	/* Head:HP MP HurtTips.... */
	UPROPERTY(VisibleDefaultsOnly, Category = UI)		UCSHead* Head;

	UPROPERTY(VisibleDefaultsOnly, Category = Info)		UCSGameCharacter* CharacterInfo;
public:
	UFUNCTION(BlueprintPure, Category = Skill) FORCEINLINE UCSSkillComponent* GetSkillComponent() { return Skill; }
	UFUNCTION(BlueprintPure, Category = Move) FORCEINLINE UActorInterpMovementComponent* GetInterpMovement() { return InterpMovement; }
	UFUNCTION(BlueprintPure, Category = Head) FORCEINLINE UWidgetComponent* GetHeadWidget() { return HeadWidget; }
	UFUNCTION(BlueprintPure, Category = Head) FORCEINLINE UCSHead* GetHead() { return Head; }
	UFUNCTION(BlueprintPure, Category = Info) FORCEINLINE UCSGameCharacter* GetCharacterInfo() { return CharacterInfo; }
public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Test) float TestComboRotateTime = 0.1f;

};