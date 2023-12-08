using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Token
{
    public enum Type
    {
        EspacoEmBranco,
        TipoVariavel,
        PontoEvirgula,
        PalavraReservada,
        NomeVariavel,
        Operador,
        OperadorRelacional,
        OperadorLogico,
        ParenteseAberto,
        ParenteseFechado,
        Numero,
        ChaveAberta,
        ChaveFechada,
        EOF // Fim do arquivo
    }

    public Type TipoToken { get; }
    public string Valor { get; }

    public Token(Type tipoToken, string valor)
    {
        TipoToken = tipoToken;
        Valor = valor;
    }
}

public class Lexer
{
    private readonly string _input;
    private int _posicao;

    public Lexer(string input)
    {
        _input = input;
        _posicao = 0;
    }

    private char CaractereAtual => _posicao < _input.Length ? _input[_posicao] : '\0';

    private void Avancar()
    {
        _posicao++;
    }

    private Token ObterProximoToken()
    {
        var caractereAtual = CaractereAtual;

        if (_posicao >= _input.Length)
        {
            return new Token(Token.Type.EOF, "");
        }

        if (char.IsWhiteSpace(caractereAtual))
        {
            while (char.IsWhiteSpace(CaractereAtual))
            {
                Avancar();
            }
            return new Token(Token.Type.EspacoEmBranco, "");
        }

        try
        {

            if (char.IsLetter(caractereAtual))
            {
                string identificador = "";
                while (char.IsLetterOrDigit(CaractereAtual))
                {
                    identificador += CaractereAtual;
                    Avancar();
                }

                if (identificador == "if" || identificador == "return" || identificador == "true")
                {
                    return new Token(Token.Type.PalavraReservada, identificador);
                }
                else if (identificador == "var")
                {
                    return new Token(Token.Type.TipoVariavel, identificador);
                }
                else
                {
                    return new Token(Token.Type.NomeVariavel, identificador);
                }

            }

            if (char.IsNumber(caractereAtual))
            {
                string identificador = "";
                while (char.IsNumber(CaractereAtual))
                {
                    identificador += CaractereAtual;
                    Avancar();
                }

                return new Token(Token.Type.Numero, identificador);
            }

            if (caractereAtual == '(')
            {
                Avancar();
                return new Token(Token.Type.ParenteseAberto, "(");
            }

            if (caractereAtual == ')')
            {
                Avancar();
                return new Token(Token.Type.ParenteseFechado, ")");
            }

            if (caractereAtual == '{')
            {
                Avancar();
                return new Token(Token.Type.ChaveAberta, caractereAtual.ToString());
            }

            if (caractereAtual == '}')
            {
                Avancar();
                return new Token(Token.Type.ChaveFechada, caractereAtual.ToString());
            }

            if (caractereAtual == ';')
            {
                Avancar();
                return new Token(Token.Type.PontoEvirgula, caractereAtual.ToString());
            }

            if (caractereAtual == '!')
            {
                Avancar();
                if (CaractereAtual == '=')
                {
                    Avancar();
                    return new Token(Token.Type.OperadorRelacional, "!=");
                }
                else
                {
                    throw new Exception("Esperado '=' após '!'");
                }
            }

            if (caractereAtual == '>' || caractereAtual == '<' || caractereAtual == '=')
            {
                Avancar();
                if (CaractereAtual == '=')
                {
                    Avancar();
                    return new Token(Token.Type.OperadorRelacional, caractereAtual.ToString() + "=");
                }
                else
                {
                    return new Token(Token.Type.OperadorRelacional, caractereAtual.ToString());
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception("Erro léxico: Caractere inválido encontrado: " + caractereAtual);
        }

        throw new Exception("Erro léxico: Caractere inválido encontrado: " + caractereAtual);
    }

    public List<Token> Tokenizar()
    {
        var tokens = new List<Token>();
        Token token;
        do
        {
            token = ObterProximoToken();
            tokens.Add(token);
        } while (token.TipoToken != Token.Type.EOF);

        return tokens;
    }
}

public class Parser
{
    private readonly List<Token> _tokens;
    private int _indiceTokenAtual;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        _indiceTokenAtual = 0;
    }

    private Token Peek()
    {
        return _tokens[_indiceTokenAtual];
    }

    private Token ObterProximoToken()
    {
        if (_indiceTokenAtual < _tokens.Count - 1)
        {
            _indiceTokenAtual++;
            return _tokens[_indiceTokenAtual];
        }
        return new Token(Token.Type.EOF, "");
    }

    public bool InstrucaoSintaticaValida()
    {
        string erroAo = "encontrar instrução if.";
        // Verifica a sequência de tokens até encontrar um 'if'
        while (Peek().Valor != "if" && Peek().TipoToken != Token.Type.EOF)
        {
            ObterProximoToken();
        }

        if (Peek().Valor == "if")
        {
            ObterProximoToken();
            erroAo = "encontrar parêntese de abertura.";
            if (Peek().TipoToken == Token.Type.ParenteseAberto || Peek().TipoToken == Token.Type.EspacoEmBranco)
            {
                if (Peek().TipoToken == Token.Type.EspacoEmBranco)
                {
                    while (Peek().TipoToken == Token.Type.EspacoEmBranco)
                    {
                        ObterProximoToken();
                    }
                }
                else if (Peek().TipoToken == Token.Type.ParenteseAberto)
                {
                    ObterProximoToken();
                }
                erroAo = "verificar nome de variável ou número";
                if (Peek().TipoToken == Token.Type.NomeVariavel || Peek().TipoToken == Token.Type.Numero)
                {
                    ObterProximoToken();
                    if (Peek().TipoToken == Token.Type.EspacoEmBranco)
                    {
                        while (Peek().TipoToken == Token.Type.EspacoEmBranco)
                        {
                            ObterProximoToken();
                        }

                        erroAo = "encontrar operador relacional.";
                        if (Peek().TipoToken == Token.Type.OperadorRelacional)
                        {
                            ObterProximoToken();
                            if (Peek().TipoToken == Token.Type.EspacoEmBranco)
                            {
                                while (Peek().TipoToken == Token.Type.EspacoEmBranco)
                                {
                                    ObterProximoToken();
                                }

                                erroAo = "verificar segunda variável ou número.";
                                if (Peek().TipoToken == Token.Type.NomeVariavel || Peek().TipoToken == Token.Type.Numero)
                                {
                                    ObterProximoToken();
                                    erroAo = "encontrar parêntese de fechamento.";
                                    if (Peek().TipoToken == Token.Type.ParenteseFechado)
                                    {
                                        ObterProximoToken();
                                        erroAo = "encontrar parêntese de abertura.";
                                        if (Peek().TipoToken == Token.Type.ChaveAberta)
                                        {
                                            int limiteIteracoes = 10;
                                            int iteracoes = 0;
                                            while (Peek().TipoToken != Token.Type.ChaveFechada && iteracoes < limiteIteracoes)
                                            {
                                                ObterProximoToken();
                                            }
                                            erroAo = "encontrar chave de fechamento.";
                                            if (Peek().TipoToken == Token.Type.ChaveFechada)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        throw new Exception($"Erro sintático: Erro ao {erroAo}");
    }

    public static void VerificarSemantica(List<Token> tokens)
    {
        var variaveisDeclaradas = new HashSet<string>();
        bool dentroDoIf = false;

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            if (token.TipoToken == Token.Type.PalavraReservada || token.TipoToken == Token.Type.ParenteseFechado)
            {
                if (token.Valor == "if")
                {
                    dentroDoIf = true;
                }
                else if (token.Valor == ")")
                {
                    dentroDoIf = false;
                }
            }
            else if (dentroDoIf)
            {
                if (token.TipoToken == Token.Type.NomeVariavel)
                {
                    string identificador = token.Valor;

                    // Verifica se a variável foi declarada antes do uso dentro do 'if'
                    if (!variaveisDeclaradas.Contains(identificador))
                    {
                        throw new Exception($"Erro semântico: A variável '{identificador}' dentro do 'if' não foi declarada.");
                    }
                }
            }
            else if (token.TipoToken == Token.Type.NomeVariavel)
            {
                string identificador = token.Valor;

                // Adiciona à lista de variáveis declaradas fora do 'if'
                variaveisDeclaradas.Add(identificador);
            }
        }
    }

    public class TabelaSimbolos
    {
        public Token Token { get; }
        public TabelaSimbolos(Token token)
        {
            this.Token = token;
        }
    }
    class Programa
    {
        static void Main(string[] args)
        {
            try
            {
                string caminhoArquivo = @"D:\Usuario\OneDrive\Documents\ArquivoA3Compilador.txt";
                string codigoInput = File.ReadAllText(caminhoArquivo);

                Lexer lexer = new Lexer(codigoInput);
                List<Token> tokens = lexer.Tokenizar();

                Console.WriteLine($"Tabela de símbolos");
                Console.WriteLine("=-=-=-=-=");
                foreach (Token token in tokens)
                {
                    Console.WriteLine($"Tipo: {token.TipoToken}. Token: {token.Valor}");
                }
                Console.WriteLine("=-=-=-=-=");
                Console.WriteLine("Análise léxica concluída com sucesso!");
                Console.WriteLine("=-=-=-=-=");
                Console.WriteLine("");

                Parser parser = new Parser(tokens);
                bool instrucaoIfValida = parser.InstrucaoSintaticaValida();
                Console.WriteLine("=-=-=-=-=");
                Console.WriteLine("Análise sintática concluída com sucesso!");
                Console.WriteLine("=-=-=-=-=");
                Console.WriteLine("");


                Console.WriteLine("=-=-=-=-=");
                Console.WriteLine($"A condição 'if' é válida sintaticamente? {instrucaoIfValida}");
                Console.WriteLine("=-=-=-=-=");
                Console.WriteLine("");


                VerificarSemantica(tokens);
                Console.WriteLine("=-=-=-=-=");
                Console.WriteLine("=-= Análise semântica concluída com sucesso! =-=");
                Console.WriteLine("=-=-=-=-=");
                Console.WriteLine("");


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();

        }
    }

}
