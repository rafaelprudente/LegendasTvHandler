namespace LegendasTvHandler
{
    public class Subtitle
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Language { get; set; }
        public string Downloads { get; set; }
        public string Score { get; set; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as Subtitle);
        }
        public bool Equals(Subtitle obj)
        {
            return obj != null && Name.Equals(obj.Name) && Language.Equals(obj.Language);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
