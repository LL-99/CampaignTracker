using CampaignTracker.Model.Creatures;

namespace CampaignTracker.Model.Combats
{
    public enum ActionType
    {
        Damage,
        Heal,
        Buff,
    }

    public abstract class ActionLogEntry(Guid Combat, Guid[] ActionPerformer, Guid[] ActionReceiver, ActionType ActionType, float ActionStrength);

    public class ActionLogEntry_Damage(Guid Combat, Guid[] ActionPerformer, Guid[] ActionReceiver, ActionType ActionType, float ActionStrength, int DamageType) : 
        ActionLogEntry(Combat, ActionPerformer, ActionReceiver, ActionType, ActionStrength) { }
}
