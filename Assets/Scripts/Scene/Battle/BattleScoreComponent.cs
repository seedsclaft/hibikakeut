using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class BattleScoreComponent : MonoBehaviour
    {
        [SerializeField] private GameObject battleScoreObj = null;
        [SerializeField] private TextMeshProUGUI battleScoreText = null;
        [SerializeField] private GameObject battleTurnObj = null;
        [SerializeField] private TextMeshProUGUI battleTurnText = null;
        [SerializeField] private GameObject battleMaxDamageObj = null;
        [SerializeField] private TextMeshProUGUI battleMaxDamageText = null;
        [SerializeField] private GameObject battleAttackPerObj = null;
        [SerializeField] private TextMeshProUGUI battleAttackPerText = null;
        [SerializeField] private GameObject battleDefeatedCountObj = null;
        [SerializeField] private TextMeshProUGUI battleDefeatedCountText = null;
        [SerializeField] private GameObject weakAttackCountObj = null;
        [SerializeField] private TextMeshProUGUI weakAttackCountText = null;
        public void UpdateScore(BattleScore battleScore)
        {
            if (battleScore == null)
            {
                return;
            }
            battleScoreObj?.SetActive(battleScore.ResultScore != -1);
            battleTurnObj?.SetActive(battleScore.TurnCount != -1);
            battleMaxDamageObj?.SetActive(battleScore.MaxDamage != -1);
            battleAttackPerObj?.SetActive(battleScore.RemainHpPercent != -1);
            battleDefeatedCountObj?.SetActive(battleScore.DefeatedCount != -1);
            weakAttackCountObj?.SetActive(battleScore.WeakAttackCount != -1);

            battleScoreText?.SetText((battleScore.ResultScore > 0 ? "+" : "") + (battleScore.ResultScore * 0.01f).ToString("F2") + "%");
            battleTurnText?.SetText(battleScore.TurnCount.ToString() + DataSystem.GetText(20301));
            battleMaxDamageText?.SetText(battleScore.MaxDamage.ToString());
            battleDefeatedCountText?.SetText(battleScore.DefeatedCount.ToString());
            weakAttackCountText?.SetText(battleScore.WeakAttackCount.ToString());
            battleAttackPerText?.SetText(battleScore.RemainHpPercent.ToString() + "%");
        }

        public void UpdateEmpty()
        {
            battleScoreObj?.SetActive(false);
            battleTurnObj?.SetActive(false);
            battleMaxDamageObj?.SetActive(false);
            battleAttackPerObj?.SetActive(false);
            battleDefeatedCountObj?.SetActive(false);
            weakAttackCountObj?.SetActive(false);
        }
    }
}
