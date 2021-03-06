﻿/****************************************************************************
Copyright (c) 2013-2015 scutgame.com

http://www.scutgame.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using System.Data;
using ZyGames.Framework.Common;

using ZyGames.Framework.Game.Service;
using ZyGames.Tianjiexing.Lang;
using ZyGames.Tianjiexing.Model;
using ZyGames.Tianjiexing.BLL.Combat;


namespace ZyGames.Tianjiexing.BLL.Action
{

    /// <summary>
    /// 5402_Boss战报鼓舞接口
    /// </summary>
    public class Action5402 : BaseAction
    {
        private const int ExpNum = 200;
        private const int GlodNum = 20;
        private int ops;
        private double inspirePercent;
        private int _activeId;


        public Action5402(HttpGet httpGet)
            : base(ActionIDDefine.Cst_Action5402, httpGet)
        {

        }

        public override void BuildPacket()
        {
            this.PushIntoStack((inspirePercent * 100).ToInt());

        }

        public override bool GetUrlElement()
        {
            if (httpGet.GetInt("Ops", ref ops)
                && httpGet.GetInt("ActiveId", ref _activeId))
            {
                return true;
            }
            return false;
        }

        public override bool TakeAction()
        {
            if (ContextUser.VipLv < 3)
            {
                this.ErrorCode = LanguageManager.GetLang().ErrorCode;
                this.ErrorInfo = LanguageManager.GetLang().St_VipNotEnough;
                return false;
            }
            if (CombatHelper.IsBossKill(_activeId))
            {
                this.ErrorCode = LanguageManager.GetLang().ErrorCode;
                this.ErrorInfo = LanguageManager.GetLang().St5405_BossKilled;
                return false;
            }
            BossCombat bossCombat = new BossCombat(_activeId);
            GameActive gameActive = bossCombat.GameActive;
            CombatStatus combatStatus = gameActive.CombatStatus;
            if (combatStatus != CombatStatus.Wait && combatStatus != CombatStatus.Combat)
            {
                this.ErrorCode = LanguageManager.GetLang().ErrorCode;
                this.ErrorInfo = LanguageManager.GetLang().St5402_CombatNoStart;
                return false;
            }
            this.ErrorCode = ops;
            if (ops == 1)
            {
                this.ErrorInfo = string.Format(LanguageManager.GetLang().St5202_InspireTip, ExpNum);
            }
            else if (ops == 2)
            {
                if (ContextUser.ExpNum < ExpNum)
                {
                    this.ErrorCode = LanguageManager.GetLang().ErrorCode;
                    this.ErrorInfo = LanguageManager.GetLang().St_ExpNumNotEnough;
                    return false;
                }
                if (!bossCombat.Inspire(Uid, false, out inspirePercent))
                {
                    this.ErrorCode = LanguageManager.GetLang().ErrorCode;
                    this.ErrorInfo = LanguageManager.GetLang().St_InspireFailed;
                }
                ContextUser.ExpNum = MathUtils.Subtraction(ContextUser.ExpNum, ExpNum, 0);
                //ContextUser.Update();
            }
            else if (ops == 3)
            {
                this.ErrorInfo = string.Format(LanguageManager.GetLang().St5202_InspireGoldTip, GlodNum);
            }
            else if (ops == 4)
            {
                if (ContextUser.GoldNum < GlodNum)
                {
                    this.ErrorCode = LanguageManager.GetLang().ErrorCode;
                    this.ErrorInfo = LanguageManager.GetLang().St_GoldNotEnough;
                    return false;
                }
                if (!bossCombat.Inspire(Uid, true, out inspirePercent))
                {
                    ErrorCode = LanguageManager.GetLang().ErrorCode;
                    ErrorInfo = LanguageManager.GetLang().St_InspireFailed;
                }
                ContextUser.UseGold = MathUtils.Addition(ContextUser.UseGold, GlodNum, int.MaxValue);
                //ContextUser.Update();
            }
            return true;
        }
    }
}