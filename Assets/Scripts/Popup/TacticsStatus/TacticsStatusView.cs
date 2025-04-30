using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Ryneus
{
    public class TacticsStatusView : BaseView
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private Button decideButton = null;
        [SerializeField] private OnOffButton characterListButton = null;
        [SerializeField] private Button leftButton = null;
        [SerializeField] private Button rightButton = null;
        [SerializeField] private Button helpButton = null;
        [SerializeField] private GameObject decideAnimation = null;
        [SerializeField] private MagicList magicList = null;
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private StatusLevelUp statusLevelUp = null;
        
        [SerializeField] private StatusAnimation statusAnimation = null;
        [SerializeField] private GameObject leftRoot = null;
        [SerializeField] private GameObject rightRoot = null;

        public SkillInfo SelectMagic => (SkillInfo)magicList.ListData?.Data;
        public override void Initialize()
        {
        }
    }
}