namespace Yapp
{
    /// <summary>
    /// Tempaltes for the prefab settings.
    /// Note that if you add a new attribute, you need to apply it in the constructor of the PrefabSettings class.
    /// </summary>
    public static class PrefabTemplates
    {
        public struct Template
        {
            public enum Type
            {
                Default, Object, Plant, Rock, House, Fence
            }

            public Type TypeId { get; set; }
            public string Name { get; set; }
            public PrefabSettings Settings { get; set; }
        }

        #region template definitions
        public static Template defaultTemplate = new Template()
        {
            TypeId = Template.Type.Default,
            Name = "",
            Settings = new PrefabSettings()
            {
                changeScale = false,
                randomRotation = false
            }
        };

        public static Template objectTemplate = new Template()
        {
            TypeId = Template.Type.Object,
            Name = "Object",
            Settings = new PrefabSettings()
            {
                changeScale = false,
                randomRotation = false
            }
        };

        public static Template plantTemplate = new Template()
        {
            TypeId = Template.Type.Plant,
            Name = "Plant",
            Settings = new PrefabSettings()
            {
                changeScale = true,
                randomRotation = false
            }
        };

        public static Template rockTemplate = new Template()
        {
            TypeId = Template.Type.Rock,
            Name = "Rock",
            Settings = new PrefabSettings()
            {
                changeScale = true,
                randomRotation = true
            }
        };

        public static Template houseTemplate = new Template()
        {
            TypeId = Template.Type.House,
            Name = "House",
            Settings = new PrefabSettings()
            {
                changeScale = false,
                randomRotation = false
            }
        };

        public static Template fenceTemplate = new Template()
        {
            TypeId = Template.Type.Fence,
            Name = "Fence",
            Settings = new PrefabSettings()
            {
                changeScale = false,
                randomRotation = false
            }
        };
        #endregion template definitions
    }
}