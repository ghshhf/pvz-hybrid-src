public interface IFlashStepZombie
{
	FlashStepComponent FlashStep { get; }

	bool IsFlashingStep { get; }

	bool CanFlashStep { get; }

	void OnFlashStepFell();
}
