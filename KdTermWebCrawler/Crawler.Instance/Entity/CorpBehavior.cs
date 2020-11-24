using System;
using System.Collections;
using System.Collections.Generic;

namespace Crawler.Instance
{
    [Serializable]
   public class CorpBehavior
    {
        private string _id;

        public virtual string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private string _corpName;

        public virtual string CorpName
        {
            get { return _corpName; }
            set { _corpName = value; }
        }
        private string _corpType;

        public virtual string CorpType
        {
            get { return _corpType; }
            set { _corpType = value; }
        }
        private string _behavior;

        public virtual string Behavior
        {
            get { return _behavior; }
            set { _behavior = value; }
        }
        private string _behaviorCtx;

        public virtual string BehaviorCtx
        {
            get { return _behaviorCtx; }
            set { _behaviorCtx = value; }
        }
        private string _othery1;

        public virtual string Othery1
        {
            get { return _othery1; }
            set { _othery1 = value; }
        }
        private string _othery2;

        public virtual string Othery2
        {
            get { return _othery2; }
            set { _othery2 = value; }
        }
        private string _othery3;

        public virtual string Othery3
        {
            get { return _othery3; }
            set { _othery3 = value; }
        }
        private DateTime? _beginDate;

        public virtual DateTime? BeginDate
        {
            get { return _beginDate; }
            set { _beginDate = value; }
        }
        public CorpBehavior()
        {
        }
        public CorpBehavior(string pid, string pcorpName, string pcorpType, string pbehavior, string pbehaviorCtx, string pothery1, string pothery2, string pothery3, DateTime? pbeginDate) {
            this._id = pid;
            this._corpName = pcorpName;
            this._corpType = pcorpType;
            this._behavior = pbehavior;
            this._behaviorCtx = pbehaviorCtx;
            this._othery1 = pothery1;
            this._othery2 = pothery2;
            this._othery3 = pothery3;
            this._beginDate = pbeginDate;
        }
        public CorpBehavior(string pId)
        {
            this._id = pId;
        }
    }
}
