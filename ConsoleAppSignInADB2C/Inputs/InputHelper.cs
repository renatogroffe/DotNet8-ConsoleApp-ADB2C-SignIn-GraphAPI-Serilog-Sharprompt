using Microsoft.Extensions.Configuration;
using Sharprompt;

namespace ConsoleAppSignInADB2C.Inputs;

public class InputHelper
{
    public static string GetUserFlow(IEnumerable<string> userFlows)
    {
        Console.WriteLine();
        var userFlow = Prompt.Select("User Flow", userFlows);
        Console.WriteLine();
        return userFlow;
    }

    public static string GetAnswerContinue()
    {
        Console.WriteLine();
        var answer = Prompt.Select("Continuar?", new[] { "Sim", "Nao" });
        Console.WriteLine();
        return answer;
    }
}