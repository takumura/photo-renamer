namespace PhotoRenamer
{
    using CommandLine;

    public class CommandLineOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Optional. Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('i', "input", Required = false, HelpText = "Optional. Set target directry(full path) which has photos to rename.")]
        public string InputDirectry { get; set; }

        [Option('c', "category", Required = false, HelpText = "Optional. Set category string aiming to identify the instruments taking photos. If this option is not set, Current directly name is automatically used.")]
        public string Category { get; set; }

        [Option('p', "preview", Required = false, HelpText = "Optinonal. Preview the process, not actually rename files")]
        public bool Preview { get; set; }
    }
}
