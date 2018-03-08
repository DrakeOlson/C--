/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: The Parser will check each token to see if it fits in the C-- Grammar
 */

using System;
namespace Compiler
{
    public class Parser
    {
        Lexer l = null;
        /// <summary>
        /// Default Constructor to create a Parser Object
        /// </summary>
        /// <param name="fileName"></param>
        public Parser(string fileName)
        {
            l = new Lexer(fileName);
            l.GetNextToken();
        }

        /// <summary>
        /// Run starts the parser. It is supposed to run after priming the parser which is done in the constructor.
        /// </summary>
        public void Run()
        {
            Prog();
            if(Globals.Token != Globals.Symbol.eoft)
            {
                Console.WriteLine($"Error: Line {Globals.LineNumber+1}: Reached end of file token with remaining tokens left over");
            }
        }

        /// <summary>
        /// Prog() handels the grammar PROG	->	TYPE idt REST PROG | lambda
        /// </summary>
        private void Prog()
        {
            if (Globals.Token == Globals.Symbol.voidT || Globals.Token == Globals.Symbol.intT || Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.charT)
            {
                Type();
                Match(Globals.Symbol.idT);
                Rest();
                Prog();
            }
            else
            {
                //Lambda
            }
        }

        /// <summary>
        /// Type() handels the grammar rule TYPE ->	int | float | char | void
        /// </summary>
        private void Type()
        {
            if (Globals.Token == Globals.Symbol.voidT)
            {
                Match(Globals.Symbol.voidT);
            }
            else if(Globals.Token == Globals.Symbol.intT)
            {
                Match(Globals.Symbol.intT);
            }
            else if(Globals.Token == Globals.Symbol.floatT)
            {
                Match(Globals.Symbol.floatT);
            }
            else if(Globals.Token == Globals.Symbol.charT)
            {
                Match(Globals.Symbol.charT);
            }
        }
        /// <summary>
        /// Rest() handels the grammar rule REST	->	( PARAMLIST ) COMPOUND | IDTAIL ; PROG
        /// </summary>
        private void Rest()
        {
            if(Globals.Token == Globals.Symbol.lparenT)
            {
                Match(Globals.Symbol.lparenT);
                ParameterList();
                Match(Globals.Symbol.rparenT);
                Compound();
            }
            else
            {
                IdTail();
                Match(Globals.Symbol.semicolonT);
                Prog();
            }
        }
        /// <summary>
        /// ParameterList() handels the rule PARAMLIST	->	TYPE idt PARAMTAIL | lambda
        /// </summary>
        private void ParameterList()
        {
            if(Globals.Token == Globals.Symbol.voidT || Globals.Token == Globals.Symbol.intT || Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.charT)
            {
                Type();
                Match(Globals.Symbol.idT);
                ParamTrail();
            }
            else{
                //Lambda
            }
        }
        /// <summary>
        /// ParamTrail() handels the rule PARAMTAIL	->	, TYPE idt PARAMTAIL | lamda
        /// </summary>
        private void ParamTrail(){
            if(Globals.Token == Globals.Symbol.commaT)
            {
                Match(Globals.Symbol.commaT);
                Type();
                Match(Globals.Symbol.idT);
                ParamTrail();
            }
            else
            {
                //Lambda
            }
        }
        /// <summary>
        /// Compound() handels the rule COMPOUND	->	{ DECL STAT_LIST	RET_STAT }
        /// </summary>
        private void Compound()
        {
            Match(Globals.Symbol.lbraceT);
            Decl();
            StatList();
            RetList();
            Match(Globals.Symbol.rbraceT);
        }
        /// <summary>
        /// Decl() handels the rule DECL	->	TYPE IDLIST | lambda
        /// </summary>
        private void Decl()
        {
            if (Globals.Token == Globals.Symbol.voidT || Globals.Token == Globals.Symbol.intT || Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.charT)
            {
                Type();
                IdList();
            }
            else
            {
                //Lambda    
            }
        }
        /// <summary>
        /// IdList() handels the rule IDLIST	->	idt IDTAIL ; DECL
        /// </summary>
        private void IdList()
        {
            Match(Globals.Symbol.idT);
            IdTail();
            Match(Globals.Symbol.semicolonT);
            Decl();
        }
        /// <summary>
        /// IdTail() handels the grammar rule IDTAIL	->	, idt IDTAIL | lambda
        /// </summary>
        private void IdTail()
        {
            if(Globals.Token == Globals.Symbol.commaT){
                Match(Globals.Symbol.commaT);
                Match(Globals.Symbol.idT);
                IdTail();
            }
            else
            {
                //lambda    
            }
        }

        /// <summary>
        /// StatList() handels the grammar rule STAT_LIST -> Lambda
        /// </summary>
        private void StatList()
        {
            //lambda
        }
        /// <summary>
        /// RetList() handels the grammar rule RET_LIST -> Lambda
        /// </summary>
        private void RetList()
        {
            //lambda
        }
        /// <summary>
        /// Match() Matches the current token to which is expected in the grammar
        /// </summary>
        /// <param name="token"></param>
        private void Match(Globals.Symbol token)
        {
            if(token == Globals.Token)
            {
                l.GetNextToken();
            }
            else{
                Console.WriteLine($"Error: Line {Globals.LineNumber+1}: Expecting {token.ToString()}. Received {Globals.Token.ToString()}");
                Environment.Exit(-1);
            }
        }
    }
}
