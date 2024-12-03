namespace Server.Models
{
    public class ScimFilterCondition
    {
        public string Attribute { get; set; }
        public string Operator { get; set; } 
        public string Value { get; set; }
    }
}
