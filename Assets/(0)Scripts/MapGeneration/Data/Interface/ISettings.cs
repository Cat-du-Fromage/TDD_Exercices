namespace KaizerWaldCode.MapGeneration.Data.Interface
{
    public interface ISettings<in T>
    where T : struct
    {
        public void CheckValues();
        public void NewGame(T input);
    }
}