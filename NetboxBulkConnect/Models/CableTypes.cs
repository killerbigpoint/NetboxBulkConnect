namespace NetboxBulkConnect.Models
{
    public class CableTypeResponse
    {
        public string name { get; set; }
        public string description { get; set; }
        public string[] renders { get; set; }
        public string[] parses { get; set; }
        public Actions actions { get; set; }
    }

    public class Actions
    {
        public POST POST { get; set; }
    }

    public class POST
    {
        public Id id { get; set; }
        public Url url { get; set; }
        public Termination_A_Type termination_a_type { get; set; }
        public Termination_A_Id termination_a_id { get; set; }
        public Termination_A termination_a { get; set; }
        public Termination_B_Type termination_b_type { get; set; }
        public Termination_B_Id termination_b_id { get; set; }
        public Termination_B termination_b { get; set; }
        public CableTypeType type { get; set; }
        public CableStatus status { get; set; }
        public Label label { get; set; }
        public Color color { get; set; }
        public Length length { get; set; }
        public Length_Unit length_unit { get; set; }
        public Tags tags { get; set; }
    }

    public class Id
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
    }

    public class Url
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
    }

    public class Termination_A_Type
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public Choice[] choices { get; set; }
    }

    public class Choice
    {
        public string value { get; set; }
        public string display_name { get; set; }
    }

    public class Termination_A_Id
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public int min_value { get; set; }
        public int max_value { get; set; }
    }

    public class Termination_A
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
    }

    public class Termination_B_Type
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public Choice1[] choices { get; set; }
    }

    public class Choice1
    {
        public string value { get; set; }
        public string display_name { get; set; }
    }

    public class Termination_B_Id
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public int min_value { get; set; }
        public int max_value { get; set; }
    }

    public class Termination_B
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
    }

    public class CableTypeType
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public CableTypeChoices[] choices { get; set; }
    }

    public class CableTypeChoices
    {
        public string value { get; set; }
        public string display_name { get; set; }
    }

    public class CableStatus
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public CableChoices[] choices { get; set; }
    }

    public class CableChoices
    {
        public string value { get; set; }
        public string display_name { get; set; }
    }

    public class Label
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public int max_length { get; set; }
    }

    public class Color
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public int max_length { get; set; }
    }

    public class Length
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public int min_value { get; set; }
        public int max_value { get; set; }
    }

    public class Length_Unit
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public Choice4[] choices { get; set; }
    }

    public class Choice4
    {
        public string value { get; set; }
        public string display_name { get; set; }
    }

    public class Tags
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public Child child { get; set; }
    }

    public class Child
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public Children children { get; set; }
    }

    public class Children
    {
        public Id1 id { get; set; }
        public Url1 url { get; set; }
        public Name name { get; set; }
        public Slug slug { get; set; }
        public Color1 color { get; set; }
    }

    public class Id1
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
    }

    public class Url1
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
    }

    public class Name
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public int max_length { get; set; }
    }

    public class Slug
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public int max_length { get; set; }
    }

    public class Color1
    {
        public string type { get; set; }
        public bool required { get; set; }
        public bool read_only { get; set; }
        public string label { get; set; }
        public int max_length { get; set; }
    }
}
