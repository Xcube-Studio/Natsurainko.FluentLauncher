namespace FluentLauncher.Models
{
    public class JavaRuntimeEnvironment
    {
        public string Title { get; set; } = "Java(TM) Platform SE Binary";

        public string Path { get; set; }

        public override bool Equals(object obj)
        {
            var item = (JavaRuntimeEnvironment)obj;
            if (this.Title == item.Title && this.Path == item.Path)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.Title.GetHashCode() ^ this.Path.GetHashCode();
        }
    }
}
