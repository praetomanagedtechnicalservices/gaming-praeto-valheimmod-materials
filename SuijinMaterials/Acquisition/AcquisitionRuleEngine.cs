using System;
using System.Collections.Generic;
using System.Linq;

namespace SuijinMaterials.Acquisition
{
    public sealed class AcquisitionRuleEngine
    {
        private readonly IReadOnlyList<AcquisitionDefinition> rules;
        private readonly AcquisitionSettingsSnapshot settings;
        private readonly IRandomSource random;

        public AcquisitionRuleEngine(IReadOnlyList<AcquisitionDefinition> rules, AcquisitionSettingsSnapshot settings, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public IReadOnlyList<DropStack> Apply(AcquisitionContext context, IReadOnlyList<DropStack> generatedDrops)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var output = (generatedDrops ?? Array.Empty<DropStack>()).Select(drop => new DropStack(drop.PrefabId, drop.Amount)).ToList();
            if (!context.IsAuthoritative || context.AlreadyEvaluated) return output.AsReadOnly();

            foreach (AcquisitionDefinition rule in rules)
            {
                RuleSettings configured = settings.Get(rule.RuleId);
                if (!configured.Enabled || configured.Chance <= 0 || rule.Kind == AcquisitionRuleKind.Disabled || !Matches(rule, context)) continue;

                if (rule.RuleId == "hen-feather-conversion")
                {
                    if (Passes(configured.Chance)) ReplaceOneFromTotal(output, rule.ReplacedPrefabId, rule.MaterialPrefabId);
                    continue;
                }

                if (rule.Kind == AcquisitionRuleKind.ReplaceCreatureDrop || rule.Kind == AcquisitionRuleKind.ReplaceContainerLoot)
                {
                    if (Passes(configured.Chance)) ReplaceOneForOne(output, rule.ReplacedPrefabId, rule.MaterialPrefabId);
                    continue;
                }

                if (!Passes(configured.Chance)) continue;
                int amount = random.NextInclusive(configured.MinimumAmount, configured.MaximumAmount);
                output.Add(new DropStack(rule.MaterialPrefabId, amount));
            }

            return output.AsReadOnly();
        }

        private bool Passes(double chance) => chance >= 1 || (chance > 0 && random.NextUnit() < chance);

        private static bool Matches(AcquisitionDefinition rule, AcquisitionContext context)
        {
            if (!MatchesEvent(rule.Kind, context.EventKind)) return false;
            bool sourceMatches = rule.SourcePrefabIds.Any(source => SamePrefab(source, context.SourcePrefabId));
            if (!sourceMatches) return false;
            if (rule.MinimumSourceLevel > 0 && context.SourceLevel != rule.MinimumSourceLevel) return false;

            if (rule.Kind == AcquisitionRuleKind.AddContainerLoot)
            {
                return context.IsNaturalContainer && !context.IsDungeonOrInterior && !context.IsPlayerBuilt && !context.IsPopulated;
            }

            if (rule.Kind == AcquisitionRuleKind.ReplaceContainerLoot)
            {
                return rule.SourceLocationIds.Any(location => string.Equals(location, context.LocationId, StringComparison.OrdinalIgnoreCase));
            }

            return true;
        }

        private static bool MatchesEvent(AcquisitionRuleKind kind, AcquisitionEventKind eventKind)
        {
            if (kind == AcquisitionRuleKind.AddContainerLoot || kind == AcquisitionRuleKind.ReplaceContainerLoot) return eventKind == AcquisitionEventKind.ContainerLootGenerated;
            if (kind == AcquisitionRuleKind.AddedDestructibleDrop) return eventKind == AcquisitionEventKind.DestructibleDestroyed;
            return eventKind == AcquisitionEventKind.CreatureDrops;
        }

        private static bool SamePrefab(string expected, string actual)
        {
            return string.Equals(Normalize(expected), Normalize(actual), StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string value)
        {
            const string clone = "(Clone)";
            string result = (value ?? string.Empty).Trim();
            return result.EndsWith(clone, StringComparison.OrdinalIgnoreCase) ? result.Substring(0, result.Length - clone.Length).TrimEnd() : result;
        }

        private static void ReplaceOneFromTotal(List<DropStack> drops, string replaced, string replacement)
        {
            int total = drops.Where(drop => SamePrefab(replaced, drop.PrefabId)).Sum(drop => drop.Amount);
            if (total == 0) return;
            drops.RemoveAll(drop => SamePrefab(replaced, drop.PrefabId));
            if (total > 1) drops.Add(new DropStack(replaced, total - 1));
            drops.Add(new DropStack(replacement, 1));
        }

        private static void ReplaceOneForOne(List<DropStack> drops, string replaced, string replacement)
        {
            for (int index = 0; index < drops.Count; index++)
            {
                DropStack drop = drops[index];
                if (SamePrefab(replaced, drop.PrefabId)) drops[index] = new DropStack(replacement, drop.Amount);
            }
        }
    }
}
