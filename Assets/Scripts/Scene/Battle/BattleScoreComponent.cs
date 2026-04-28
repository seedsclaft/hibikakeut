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
        [SerializeField] private GameObject enemyLvAvarageObj = null;
        [SerializeField] private TextMeshProUGUI enemyLvAvarageText = null;
        public void UpdateScore(BattleScore battleScore)
        {
            if (battleScore == null)
            {
                return;
            }
            UIComponent.SetActive(battleScoreObj, battleScore.ResultScore != -1);
            UIComponent.SetActive(battleTurnObj, battleScore.TurnCount != -1);
            UIComponent.SetActive(battleMaxDamageObj, battleScore.MaxDamage != -1);
            UIComponent.SetActive(battleAttackPerObj, battleScore.RemainHpPercent != -1);
            UIComponent.SetActive(battleDefeatedCountObj, battleScore.DefeatedCount != -1);
            UIComponent.SetActive(weakAttackCountObj, battleScore.WeakAttackCount != -1);
            UIComponent.SetActive(enemyLvAvarageObj, battleScore.EnemyLvAvarage != -1);

            UIComponent.SetText(battleScoreText, (battleScore.ResultScore > 0 ? "+" : "") + (battleScore.ResultScore * 0.01f).ToString("F2") + "%");
            UIComponent.SetText(battleTurnText, battleScore.TurnCount.ToString() + DataSystem.GetText(20301));
            UIComponent.SetText(battleMaxDamageText, battleScore.MaxDamage.ToString());
            UIComponent.SetText(battleDefeatedCountText, battleScore.DefeatedCount.ToString());
            UIComponent.SetText(weakAttackCountText, battleScore.WeakAttackCount.ToString());
            UIComponent.SetText(battleAttackPerText, battleScore.RemainHpPercent.ToString() + "%");
            UIComponent.SetText(enemyLvAvarageText, battleScore.EnemyLvAvarage.ToString());
        }

        public void UpdateEmpty()
        {
            UIComponent.SetActive(battleScoreObj, false);
            UIComponent.SetActive(battleTurnObj, false);
            UIComponent.SetActive(battleMaxDamageObj, false);
            UIComponent.SetActive(battleAttackPerObj, false);
            UIComponent.SetActive(battleDefeatedCountObj, false);
            UIComponent.SetActive(weakAttackCountObj, false);
            UIComponent.SetActive(enemyLvAvarageObj, false);
        }
    }
}
