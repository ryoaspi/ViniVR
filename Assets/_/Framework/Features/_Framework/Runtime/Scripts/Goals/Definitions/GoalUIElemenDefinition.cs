using UnityEngine;

namespace TheFoundation.Runtime
{

    [CreateAssetMenu(fileName = "GoalUIElementDefinition", menuName = "TheFoundation/Goals/UI Element Definition")]
    public class GoalUIElementDefinition : ScriptableObject
    {
        [Header("Goal to Display")] 
        public string m_goalKey;
        
        [Header("Show Progress Bar")]
        public bool m_ShowProgress = true;

        [Header("Visual Option")] 
        public Sprite m_icons;

    }
    
}
