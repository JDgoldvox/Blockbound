using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Tiles/SpecialCustomRuleTile")]
public class SpecialCustomRuleTile : RuleTile<SpecialCustomRuleTile.Neighbor> {

    public enum RuleMatchType
    {
        SameOnly,
        Anything,
        Specific
    }
    
    public RuleMatchType matchType;
    public TileBase[] tilesToConnect;
    
    public class Neighbor : RuleTile.TilingRule.Neighbor {
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {

        switch (matchType)
        {
            case RuleMatchType.SameOnly:
                switch (neighbor) {
                    case Neighbor.This: return tile == this;
                    case Neighbor.NotThis: return tile != this;
                }
                break;
            
            case RuleMatchType.Anything:
                switch (neighbor) {
                    case Neighbor.This: return tile != null;
                    case Neighbor.NotThis: return tile ==null;
                }
                break;
            
            case RuleMatchType.Specific:
                switch (neighbor)
                {
                    case Neighbor.This: return tilesToConnect.Contains(tile) || tile == this;
                    case Neighbor.NotThis: return tile == null;
                }
                break;
        }
     
        return base.RuleMatch(neighbor, tile);
    }
}