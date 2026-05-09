using CampaignTracker.Model.Creatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CampaignTracker.Model.Combats
{
    public enum EffectType
    {
        Damage,
        Condition,

        TemporaryHP,
        Heal,
        Buff,
    }

    public class ActionLogEntry 
    {
        public Guid Combat { get; set; }
        public int Turn { get; set; } = 1;
        public Guid[] Actors { get; set; } = [];

        [JsonProperty(ItemConverterType = typeof(ActionEffectJsonConverter))]
        public ActionEffect[] Effects { get; set; } = [];
    }

    public class ActionEffect
    {
        public EffectType Type { get; set; }
        public string? Description { get; set; }
    }

    public class ActionEffect_Damage : ActionEffect
    {
        public ActionEffect_DamageEntry[] DamageInstances { get; set; } = [];
    }

    public class ActionEffect_DamageEntry
    {
        public Guid Target { get; set; }
        public DamageType DamageType { get; set; }
        public float BaseDamage { get; set; }
        public float DamageMultiplier { get; set; }
        public string? Note { get; set; }
    }

    public class ActionEffect_Condition : ActionEffect
    {
        public Condition Condition { get; set; }
        public Guid[] Targets { get; set; } = [];
    }

    public class ActionEffect_TemporaryHP : ActionEffect
    {
        public float TemporaryHPAmount { get; set; }
        public Guid[] Targets { get; set; } = [];
    }

    public class ActionEffect_Heal : ActionEffect
    {
        public float HealAmount { get; set; }
        public Guid[] Targets { get; set; } = [];
    }

    public class ActionEffect_Buff : ActionEffect
    {
        public Guid[] Targets { get; set; } = [];
    }

    public class ActionEffectJsonConverter : JsonConverter<ActionEffect>
    {
        public override bool CanWrite => false;

        public override ActionEffect? ReadJson(JsonReader reader, Type objectType, ActionEffect? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jsonObject = JObject.Load(reader);
            var effectType = jsonObject[nameof(ActionEffect.Type)]?.ToObject<EffectType>(serializer);
            var targetType = effectType switch
            {
                EffectType.Damage => typeof(ActionEffect_Damage),
                EffectType.Condition => typeof(ActionEffect_Condition),
                EffectType.TemporaryHP => typeof(ActionEffect_TemporaryHP),
                EffectType.Heal => typeof(ActionEffect_Heal),
                EffectType.Buff => typeof(ActionEffect_Buff),
                _ => typeof(ActionEffect)
            };

            var effect = (ActionEffect?)Activator.CreateInstance(targetType);
            if (effect is null)
            {
                return null;
            }

            serializer.Populate(jsonObject.CreateReader(), effect);
            return effect;
        }

        public override void WriteJson(JsonWriter writer, ActionEffect? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
