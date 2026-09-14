using UnityEngine;
using System.Collections;

public class Action
{
    public ActionType         mActionType;
    public World.WorldState   mPreconditions;
    public World.WorldState   mPosCondition;
    public World.WorldState   mEffects;
    public World.WorldState   mBadEffect;
    public float              mCost;
    public string             mName;

  /***************************************************************************/

  public enum ActionType
  {
    ACTION_TYPE_NONE                      = -1,
    ACTION_TYPE_STAB                          ,
    ACTION_TYPE_SHOOT                         ,
    ACTION_TYPE_LOAD_GUN                      ,
    ACTION_TYPE_PICK_UP_GUN                   ,
    ACTION_TYPE_PICK_UP_KNIFE                 ,
    ACTION_TYPE_GO_TO_ENEMY                   ,
    ACTION_TYPE_GO_TO_GUN                     ,
    ACTION_TYPE_GO_TO_KNIFE                   ,
    ACTION_TYPE_GET_LINE_OF_SIGHT_TO_ENEMY    ,
    ACTION_TYPES
  }
	
  /***************************************************************************/

	public Action( ActionType actionType, World.WorldState preconditions, World.WorldState posCondition, World.WorldState effects, World.WorldState badEffect, float cost, string name )
    {
        mActionType     = actionType;
        mPreconditions  = preconditions;
        mPosCondition   = posCondition;
        mEffects        = effects;
        mBadEffect      = badEffect;
        mCost           = cost;
        mName           = name;
    }
                                                      
  /***************************************************************************/

}
