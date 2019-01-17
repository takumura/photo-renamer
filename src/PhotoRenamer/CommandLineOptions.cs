namespace PhotoRenamer
{
    using CommandLine;

    public class CommandLineOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('i', "input", Required = false, HelpText = "Target directry which has photos and movies to rename.")]
        public string InputDirectry { get; set; }

        [Option('c', "category", Required = false, HelpText = "Category string aiming to identity the instruments")]
        public string Category { get; set; }

        [Option('p', "preview", Required = false, HelpText = "Preview the process, not actually rename files")]
        public bool Preview { get; set; }
    }
}
