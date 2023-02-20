namespace AOVGEN.ParsePreset
{
    class SetParamContext
    {
        public ISetParam ParamStrategy { get; set; }
        public EditorV2.PosInfo PosInfo { get; private set; }
        public object Entity { get; private set; }
        public SetParamContext(ISetParam param)
        {
            ParamStrategy = param;
        }
        public SetParamContext() { }
        public void SetParams()
        {
            ParamStrategy.SetParams();
            PosInfo = ParamStrategy.GetPosInfo();
            Entity = ParamStrategy.GetEntity();
        }
          
    }
}
