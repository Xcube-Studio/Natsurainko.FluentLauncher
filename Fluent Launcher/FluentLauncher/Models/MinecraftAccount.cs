namespace FluentLauncher.Models
{
    public class MinecraftAccount
    {
        public string YggdrasilServerUrl { get; set; }

        public string Type { get; set; }

        public string UserName { get; set; }

        public string Uuid { get; set; }

        public string AccessToken { get; set; }

        public string ClientToken { get; set; }

        public string RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public string Time { get; set; }

        public override bool Equals(object obj)
        {
            var item = (MinecraftAccount)obj;
            if (this.Type == item.Type && this.UserName == item.UserName &&
                this.Uuid == item.Uuid && this.AccessToken == item.AccessToken &&
                this.RefreshToken == item.RefreshToken && this.ExpiresIn == item.ExpiresIn &&
                this.Time == item.Time)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.Uuid.GetHashCode() ^ this.UserName.GetHashCode() ^ this.Type.GetHashCode();
        }

    }
}
