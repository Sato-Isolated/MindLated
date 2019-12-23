using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;

namespace Isolated.Protection.CtrlFlow
{
    public static class controlflow
    {
        public static void Intiailize(ModuleDefMD asm)
        {
            controlflow.asm = asm;
            controlflow.ldcClass = obfuscatorHelper.importType("imgjpg");
            asm.Types.Add(controlflow.ldcClass);
            renameType(controlflow.ldcClass, false);
            controlflow.intialized = true;
        }

        public static void renameType(TypeDef type, bool nested = false)
        {
            string text = generate(-1);
            if (!nested && !string.IsNullOrEmpty(type.Namespace))
            {
                foreach (Resource resource in type.Module.Resources)
                {
                    if (resource.Name.Contains(type.FullName))
                    {
                        resource.Name = resource.Name.Replace(type.FullName, text);
                    }
                }
            }
            foreach (MethodDef methodDef in type.Methods)
            {
                foreach (Parameter parameter in ((IEnumerable<Parameter>)methodDef.Parameters))
                {
                    parameter.Name = generate(-1);
                }
                if (!methodDef.IsVirtual && !methodDef.IsSpecialName && !methodDef.IsRuntimeSpecialName)
                {
                    methodDef.Name = generate(-1);
                }
            }
            foreach (PropertyDef propertyDef in type.Properties)
            {
                if (!nested)
                {
                    string text2 = generate(-1);
                    if (propertyDef.Name == "ResourceManager")
                    {
                        foreach (Instruction instruction in propertyDef.GetMethod.Body.Instructions)
                        {
                            if (instruction.OpCode == OpCodes.Ldstr && instruction.Operand.ToString().Length < 100)
                            {
                                instruction.Operand = text;
                            }
                        }
                    }
                    if (type.Name == "Settings")
                    {
                        if (propertyDef.GetMethod != null)
                        {
                            foreach (Instruction instruction2 in propertyDef.GetMethod.Body.Instructions)
                            {
                                if (instruction2.OpCode == OpCodes.Ldstr)
                                {
                                    instruction2.Operand = text2;
                                }
                            }
                        }
                        if (propertyDef.SetMethod != null)
                        {
                            foreach (Instruction instruction3 in propertyDef.SetMethod.Body.Instructions)
                            {
                                if (instruction3.OpCode == OpCodes.Ldstr)
                                {
                                    instruction3.Operand = text2;
                                }
                            }
                        }
                    }
                    propertyDef.Name = text2;
                }
            }
            foreach (EventDef eventDef in type.Events)
            {
                eventDef.Name = generate(-1);
            }
            foreach (FieldDef fieldDef in type.Fields)
            {
                fieldDef.Name = generate(-1);
            }
            if (!nested)
            {
                type.Namespace = "";
            }
            if (!nested)
            {
                type.Name = text;
            }
        }

        public static Random r = new Random();

        public static string generate(int length = -1)
        {
            string text = "‮";
            string text2 = ".̃̏̂͑̽̿̄ͯ҉̬̬̭͉͙.ͮͦ͌̑̄ͤͥͫ̕͏͉̖̪̪̤͈̝̠͓̥̹̥̺͉ͅ.̶̸̘̘̙̭̻͖̪͍̖̰͍͇̟̯̖̃͒ͧ̈́ͧ͛͑ͣ̂̌͑̽̎̔̚͢.͙̙͓͔̻̺̪͙̼̺͙̠̭̣̯̫͔̭ͥͦͫ͜͟͞.̩͈̤̼̬̬̼̘͎̻̼̠̼͓̝̯̰̌̆̋̃͗̃̅͂̂̈́̔͒̑̕͜.̧̭̫̭̮͙̣̺̳̦̝̦͍͚̟̯̟̽̂ͨ͑̈̀͟͠.̡̣̖͚̮͓͇͔͈̱̯̞͓̙̞͕͚́ͩ̾ͯ̍̏̿̆ͫ̆͛̑́͝ͅ.̛̊ͨ̆͂͌ͭ͏̸͓͕͕͓̗͙͇̤͍̦͕̥̘͇.̡͔͇͍̦͚̲͔̯̪̙̘͓͚̬̲͔̼͕̽̃̉ͫ̓̑ͫ̉ͫ̒̊͜.͎̹̫͕ͯ̐͌̐͒͛̐̎̏ͨͮ͂̒̀̚͠.̶̷̨̝̣͖͇̲̯͇̰͈̙͉͙͚͉̄͑͗̏̒ͪ̏ͮ͌͗ͬͪͥͭ̊͋͞.̜̟͕̺̣͕̥͚͔͓̠̞̳̭̠ͪ̽ͭ͒ͮ͘͝.̛̝̦͎͚̈̉̈́̈͛ͯ͑ͫ̊ͮͬ̆ͣ͂ͥ.̸͇̮͙͈͇̱͈͕̜̬̻̮ͫ͊ͭ̏͑̔͐̑ͬ̾̂ͩ͆ͫ̀.ͤ͂ͩ̀͑̒͏̷́͏͓̮̙͈̮̳̲̭̺̟̱̞͍̜̥͜.̶̷̣̮͍͇͈̝̞͓̦͐ͤͦͤͦͭ̆͒̓̀ͫ̅̐̚͡.̵̢͇̯͕͕̤̥̘͍͂̆̊ͮ̆̋̿ͧ͊ͩ̑͜.̸̧̗̜̼͖̲̟̹̞̈ͭ̊̔ͪ̐ͤ͆̇̔ͫͮ̀́.̏̅̃̓ͭ͏͇̲͎̹̖͙͎̯̥͡.̢̰͕̭̲͖͇͒́̾͋ͬ̅̈͑ͥͅͅ.͊̒̃͒́͜͏̢͉̲̹̼̥̥͖̘̼̹͈͉.̸̷̶̜̞͖̪̻̦͕͕̼̮̳͙̯̹̩̗̓͌̑ͭ̏͂̾͂̒ͭ̍̀̚.̵̡̘͕͚̳͐ͦͥ̉͘͢ͅ.̶̸̨̨͍̳̣̱͓̫̫̱̖̣͔̅ͧ̂́ͯ̓͋͋͂̾͑̈́̇̑̎̑ͭ̍͜.̴̵̘̩͍͖̻̦̣͕̗̖͔̘͓͗̈͛͂́̾ͫ͛̄͆ͤ̑͘͘.̡̛̮͇̫̮͔̲͕̫̹̘̞̱̾̈ͬ̆ͦ̈́͂̀̌̈́̆͋͆͋́.̶̴̨̩̻̮̹͔̞̻͖̭̻̲̉͆̓ͨͥ̈́̈́ͤ̅͑̆̑̔̔̍̀͘͝.̃̏̂͑̽̿̄ͯ҉̬̬̭͉͙.ͮͦ͌̑̄ͤͥͫ̕͏͉̖̪̪̤͈̝̠͓̥̹̥̺͉ͅ.̶̸̘̘̙̭̻͖̪͍̖̰͍͇̟̯̖̃͒ͧ̈́ͧ͛͑ͣ̂̌͑̽̎̔̚͢.͙̙͓͔̻̺̪͙̼̺͙̠̭̣̯̫͔̭ͥͦͫ͜͟͞.̩͈̤̼̬̬̼̘͎̻̼̠̼͓̝̯̰̌̆̋̃͗̃̅͂̂̈́̔͒̑̕͜.̧̭̫̭̮͙̣̺̳̦̝̦͍͚̟̯̟̽̂ͨ͑̈̀͟͠.̡̣̖͚̮͓͇͔͈̱̯̞͓̙̞͕͚́ͩ̾ͯ̍̏̿̆ͫ̆͛̑́͝ͅ.̛̊ͨ̆͂͌ͭ͏̸͓͕͕͓̗͙͇̤͍̦͕̥̘͇.̡͔͇͍̦͚̲͔̯̪̙̘͓͚̬̲͔̼͕̽̃̉ͫ̓̑ͫ̉ͫ̒̊͜.͎̹̫͕ͯ̐͌̐͒͛̐̎̏ͨͮ͂̒̀̚͠.̶̷̨̝̣͖͇̲̯͇̰͈̙͉͙͚͉̄͑͗̏̒ͪ̏ͮ͌͗ͬͪͥͭ̊͋͞.̜̟͕̺̣͕̥͚͔͓̠̞̳̭̠ͪ̽ͭ͒ͮ͘͝.̛̝̦͎͚̈̉̈́̈͛ͯ͑ͫ̊ͮͬ̆ͣ͂ͥ.̸͇̮͙͈͇̱͈͕̜̬̻̮ͫ͊ͭ̏͑̔͐̑ͬ̾̂ͩ͆ͫ̀.ͤ͂ͩ̀͑̒͏̷́͏͓̮̙͈̮̳̲̭̺̟̱̞͍̜̥͜.̶̷̣̮͍͇͈̝̞͓̦͐ͤͦͤͦͭ̆͒̓̀ͫ̅̐̚͡.̵̢͇̯͕͕̤̥̘͍͂̆̊ͮ̆̋̿ͧ͊ͩ̑͜.̸̧̗̜̼͖̲̟̹̞̈ͭ̊̔ͪ̐ͤ͆̇̔ͫͮ̀́.̏̅̃̓ͭ͏͇̲͎̹̖͙͎̯̥͡.̢̰͕̭̲͖͇͒́̾͋ͬ̅̈͑ͥͅͅ.͊̒̃͒́͜͏̢͉̲̹̼̥̥͖̘̼̹͈͉.̸̷̶̜̞͖̪̻̦͕͕̼̮̳͙̯̹̩̗̓͌̑ͭ̏͂̾͂̒ͭ̍̀̚.̵̡̘͕͚̳͐ͦͥ̉͘͢ͅ.̶̸̨̨͍̳̣̱͓̫̫̱̖̣͔̅ͧ̂́ͯ̓͋͋͂̾͑̈́̇̑̎̑ͭ̍͜.̴̵̘̩͍͖̻̦̣͕̗̖͔̘͓͗̈͛͂́̾ͫ͛̄͆ͤ̑͘͘.̡̛̮͇̫̮͔̲͕̫̹̘̞̱̾̈ͬ̆ͦ̈́͂̀̌̈́̆͋͆͋́.̶̴̨̩̻̮̹͔̞̻͖̭̻̲̉͆̓ͨͥ̈́̈́ͤ̅͑̆̑̔̔̍̀͘͝ḩ̷̸͎̞̬͚͙́͒̃̿̑ส็็็็็็็็็็็็็็็็็็็i͇̠̱̽͛ͣͯͭ̐͐ͩͪ̀͒̿̍̆̌ͣ̕͞ţ̈́̄ͦ͑͐ͤ̇ͯ̚͜͢͏̺͎̰̯̰̳̣̺͉͉̻̯̱͉̱̳̠̫l̢̮̝̰̖̲̯͉̱͉̤̗̯͇ͫ͋͑͋͊́͑͠e̛̼͉̝̯̼͚͇̜̹̬̼͚̥̝̟̩̮̎̾ͧ͟͝ͅr̷͎̣͙͇̦̱̺͚̬͍͎̗̺͍͈͍̔̃̆ͬ̃͌ͦ͗ͧ̓͋̓͟͟͡ͅ ̵̩̼͙̣̦͕̃ͨͧ̂ͭ͂̀͜ḣ͚͖͉͓̫̲̦͓́̆̈ͯ͒͂ͫ͛ͣ̓ͫ̄́́̕͜͜͜ą̴̢̺̼͎̩͓̱͍̯͓̻̖͓̯̿ͩͩͦ̕ͅt͂ͫ̔͋͆̀ͩͨ͂̎̓ͧ̿̈́̓̏̃ͯ̈͘͘҉͚̬̝͙̟̗̰̹̱̗ ̵̠̤̼̬̩͔̲̖̎̍̈̌̾̎̋̂̓ͬ̒ͫ̽ͭ́̕͠n̛̲͙̻̤̮̥̠͇͇͖͎̘̠̲ͥͣ̋͛ͨ̀i̧͓͖͈̭͔͉̼ͪͫ̓͂̔̿͠ͅč̨̆̉͂̑͞͏̻̠̖̼̹̻̹̯͇͙̰̪̯hͭ͌ͦ̉̊͐͂҉̸͇̹̹̬̖͕̱͕͕̠̗̀͘͝tͤͫͫͧ̈́͂͛ͭ̉ͧ͛ͫ̚҉̴͚̯̭̲̫̦͖̮̭͖̗͎̳̟̀͘s̸ͧ͗̌͊ͨ̐̅̇͟҉͈̤̘͉̤̯̝͈͚ ͤ͆͆ͨ̓҉̤̣̩̠̩̯̩̱͕̹͜͝f̡̞͔̮͖̩͔̀̆̅̓̈ͪͥ͋͊̉ͪ̇̉̃̔͋̃̈́͟͠͝ä̵̸̺̖͓͖̳̬̲̲͎͎͔̈͋͑͋ͦͅͅl̻̟̲̞̘͚̤͎͉̯̫̹̜̥̳͈̙ͧ͋̇ͫ͋̎ͯ̋͂ͮ̈͂̾ͯ̎̊̾ͯ͘͢ͅs̴̡͍̖̖͕̱̫̤̣͛ͩͤ͆̅ͣ͐̿ͣ͐̔ͨ̄ͫͩ̄̍͘̕͜c̵̷̨͇̰̰̼̝̝̼̤͎̯̺̰͕̤̤͇ͧͦ̄̇̓ͩ̎͂̊ͯ͋͋̋̀ͬͧ͗̔̚͝͡ͅͅh͓͙̩̭̬̠̜͇̗̮̐ͥͧͭ̆͆̔ͬ́̄͊ͮ͡͝͠ ̩̹̥̯̲͉͔̟͕͎̪͔̱̬̌ͭ͗̔̏̊̚͡͞g̴̴̫͖̥̲̦͉̩̲̪̹̙̘̩̣̯̜̱͌ͩ͑̆̿̏̽ͤ͂ͩ́͢͠e̤̳͈̹͉̹̪̥̜̲͙͕͍̟̱̱̳͗̽ͤ̈́̽̽̊͢͡ḿ͈̮͓͇̞̯͍̦͖̟͔̫͈̏̑̋̂̒ͬ͌̌̓̄͢͞ą̶̴͈̳̥͙͚͓͉̟̬̤͋͒ͫ̓̿́̒͐͒͘͡c̡̞̪̞̣̦̖̙̬̜̜̋̿͐̇̓̋̃͆̚h̨̛̝͓͚̱͍͕̝̬̯̩̓̽̉͌͊̇͐̒̈͋͋̌ͩ̋ͭt̶̄́͑̐҉҉̟̭̟͓̜̩͖̲͔̀ ̷̧̡̦̞̝̯̥͍̻͚̠̞̣̯͎̇̓̇̐̓́͝h̡̺̳͇̤̬̻̮̭͇̿̑̔̽ͮ̉̏̃͂̄̌̍͒̑̇ͪ̑͡͞ĭ̵͔͙̞̻̰̻̬̖͇̩͔̪̮̩̘̔ͪͥ̈́͋͞t̸̝̪̹̲̤̜̓͌ͤ̍ͫͧ͋͋ͣ͆̈́ͩͯ̒ͮ̊ͧ̕͢͜ͅl̈́ͦ̈̌͆ͧ͏̷̛̮̼̬͓̞̻̣̼͙ͅe̗͚̺̰͎͖̥̙̻͕̮͕̱͇̓̑ͫ͋ͭͯ̍ͨ̌͗̊̔̒̈́̽̕͜͝ͅṛ̶̷̟̹̥̹̬̖͖͇̬̭̲̬̠̮ͣͧ̓̓̈́̅͢ ̨̛̘̲͉̮̲̹̳͔̗̣̣̗̱̱̘͚ͦ͋̀̍̃̑ͣ̍͒̇ͭ̆ͧ͒͒̋̕h̷͔̠͈̙̝̻̺͔͕̤ͯ̃̎̋͑ͫ̎̾̃̿̀̄́̀͞a̍̏͛̔͆͗͒̆̐́̈ͥ̃͌͝҉̧҉̷̪̳̻͓̘̜̘͔̘̞̱̫͈̹ͅť̷̢̫̰̦̫̯̮̟͇̍͊̒̅̀̐͊ͯ́̒̅ͤͅ ̵̮̤̳̺̤̼̝̉̑̎ͧ̏̀̽̽́n̴̴̯̹̮̣͉̹̝̑͑̐̿ͮ̈̆̔́̏ͥ͋ͨ̒͘͘͟i̲̬͓̯̋͑̂̋͋͡c͑̒̈́ͫ̓̎͆̃̃ͬͯ͜͏͓̲̝̺̥̘͍͚̕ḩ̜͖̤̼͇̳̳̭̻͖̳̙͑͌̑̓͒ͦ͂ͮ̅̅͌̈ͭͭ̄ͅṫ̴͎̺̭̺̞̺̮̼̣͔͎͔̗̱̭̄̆̓͋̔͝s̛͓͇̮̹̙̫̮̦̪̜̋ͪ̊̊ͮ̎͌̂̂̿̽̔̉̓̍̔͆̕ ̛̽͌͋ͩ̃̈̍ͨ̅ͦ̀̏̍̓̑̍̊ͧ͘͏̪͔͇ͅf̢̰̲̺͙̝̣͕̭̝̙͍ͧ̌ͤͮ͛͋ͭ́̓̽̔̈́̂ͤ̉̆ͩ̚͘ͅa̧̡ͪ̊̊ͩ̈͠͏҉̠̬̝̻͙̰̖̻̼̖̘̠̺̝l̸̷͊͒ͨ̄̓̂҉̥̩̮̳̯̠̻͎̹͈̟̠̫̮̫̠̖̥͙s̶͒̓̇͑͗̍̿̐ͮ҉͇͖͓̣͉̗̰̯͎̖͎̱c̶͇̭̣͍̞̝͓͇̫̯̜̫̞͉̑ͤͧ̎̒̈ͯͣͥ̍ͪ̌̎̒́h̵̢̼̮̖͎̭̭͇͚̮͙͙͇̗̤̝̺͚̐̏͋ͬ̋̿̎ͭ̂̾̂̓ͪ̏̋̀ ̵̨̛̱̖̫͇̱͈̞̭̱́̔ͪ̋̎̎ͭͨ̈̿ͤ̎̿͟ͅg̉͂͛̍̓ͫ̇̿̎͐̓̏͏͏̴̤̗̙͖͙̱̺͎̖̩͝ę̵̞͖̭̳͔̻͉̯̻̯̣̈́́̈̊̽̾͗ͣ̃͊ͬ̔ṃ̶̳̫̲̩̺͍̝̰̻̱̖̦̪̘̠̏ͫ̎ͤ̓ͣ̾͊̑͒͗̋͂̉̈́͜͞a̰̯͔̗̠͖̣̬̖͐ͬ̏͐ͬ͊̂̍̇ͣͩ́͠͞c̨̡̬̥̟̩̠̬̟̪͖̙̮̺̩͍̮̝ͥ̌̉ͣ͊̈͌̓̈̉̈͐̿̈ͬ̀h̛̬͚̝͉̮̝͉̥̺̩̼̞̙̖͎̮̳̒͂̋̋ͬ̓͋̂̊̚̕t͒ͮ̿ͤͬ̎ͭ͌̅̂̾̐̉ͦ͌ͧͯ͋̈͏̮̮̰̻̕͜͞ ̸͎̥̞̟͙͚͙̥ͬ̈́̋̔̿ͣͨͧͫ͒̿ͬͫͧ̊͢͜͠ḫ̝̘̤̰͐͆ͤ̑̒͛ͩ́͞iͤ̓͑̊̏ͥ̀͘͞҉̨̗̬͉̜̘̜t̑ͨ͋̾ͦ̋̊ͤ̔̒̑̿̓́͏̵̛̱͚̖͓̲̕l̶͍̳̳͎͓̀ͭ̎̉̌̓̊̌̍̍̀̕e̵̡͇͈͉̥̼̼̺͎͉̦ͮ̾̒̃ͫ̃͒̃̓́̅͆ͬ͗ͨ̿ͥ̂̚͝͡ŗ̡̊͋̾ͩ̽͆̋̈́͐͊̂ͮͨ̉̏ͨ͐͐̆̕͞͏͍͍̰̻̖̥ ͨ́̿̃̇ͯ̾̕͢҉̩͇̟̪̥̬͍̲͈͔͔͍̼̭͇̣h̷̻̝̫̪͚̦͙͉͎̥̦̳͉̖̃ͥ̅͗͢ḁ̛̪̗̮̦̳̪̭̞̗̠̟̈́̌͐̈́ͦ̄̉̎ͬ͆̒̉̕͟t̶̛͔͍͔͎̫̞̖͓̰̒̇̆ͯ̀ͥ̈́̏̓̀ͮͥ̍̀ ̨ͥ̄̓ͩ̿̃̿̊̈̔͒͗͊ͭ̽ͥͥ͐҉̶̸̲̻̱̩̖̪̹͈̙̩͎̲̘̙n";
            if (length == -1)
            {
                length = r.Next(40, 60);
            }
            for (int i = 0; i < length; i++)
            {
                text += text2[r.Next(0, text2.Length - 1)];
            }
            return text;
        }

        public static void processMethods1(IList<MethodDef> methods)
        {
            foreach (MethodDef methodDef in methods)
            {
                if (methodDef.Body != null && methodDef.Body.Instructions.Count > 2)
                {
                    controlflow.processMethod1(methodDef);
                }
            }
        }

        public static void processMethod1(MethodDef method)
        {
            method.Body.SimplifyBranches();
            InstructionGroups instructionGroups = new InstructionGroups();
            InstructionGroup instructionGroup = new InstructionGroup();
            int num = 0;
            int num2 = 0;
            bool flag = false;
            foreach (Instruction instruction in method.Body.Instructions)
            {
                int num3 = 0;
                int num4;
                instruction.CalculateStackUsage(out num4, out num3);
                instructionGroup.Add(instruction);
                num2 += num4 - num3;
                if (num4 == 0 && instruction.OpCode != OpCodes.Nop && (num2 == 0 || instruction.OpCode == OpCodes.Ret))
                {
                    if (!flag)
                    {
                        InstructionGroup instructionGroup2 = new InstructionGroup();
                        instructionGroup2.ID = num++;
                        instructionGroup2.nextGroup = instructionGroup2.ID + 1;
                        instructionGroups.Add(instructionGroup2);
                        instructionGroup2 = new InstructionGroup();
                        instructionGroup2.ID = num++;
                        instructionGroup2.nextGroup = instructionGroup2.ID + 1;
                        instructionGroups.Add(instructionGroup2);
                        flag = true;
                    }
                    instructionGroup.ID = num++;
                    instructionGroup.nextGroup = instructionGroup.ID + 1;
                    instructionGroups.Add(instructionGroup);
                    instructionGroup = new InstructionGroup();
                }
            }
            if (instructionGroups.Count == 1)
            {
                return;
            }
            InstructionGroup last = instructionGroups.getLast();
            instructionGroups.Scramble(out instructionGroups);
            method.Body.Instructions.Clear();
            Local local = new Local(controlflow.asm.CorLibTypes.Int32);
            method.Body.Variables.Add(local);
            Instruction instruction2 = Instruction.Create(OpCodes.Nop);
            Instruction instruction3 = Instruction.Create(OpCodes.Br, instruction2);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Br, instruction3));
            method.Body.Instructions.Add(instruction2);
            foreach (InstructionGroup instructionGroup3 in instructionGroups)
            {
                if (instructionGroup3 != last)
                {
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, instructionGroup3.ID));
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
                    Instruction instruction4 = Instruction.Create(OpCodes.Nop);
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instruction4));
                    foreach (Instruction item in instructionGroup3)
                    {
                        method.Body.Instructions.Add(item);
                    }
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, instructionGroup3.nextGroup));
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
                    method.Body.Instructions.Add(instruction4);
                }
            }
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, instructionGroups.Count - 1));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instruction3));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Br, last[0]));
            method.Body.Instructions.Add(instruction3);
            foreach (Instruction item2 in last)
            {
                method.Body.Instructions.Add(item2);
            }
            method.Body.OptimizeBranches();
            method.Body.OptimizeMacros();
        }

        public static void processMethod2(MethodDef method)
        {
            method.Body.SimplifyBranches();
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                Instruction instruction = method.Body.Instructions[i];
                if (instruction.IsLdcI4())
                {
                    int num = instruction.GetLdcI4Value();
                    if (num <= 2000)
                    {
                        num += 20;
                        method.Body.Instructions[i].OpCode = OpCodes.Ldstr;
                        method.Body.Instructions[i].Operand = generate(num);
                        method.Body.Instructions.Insert(++i, Instruction.Create(OpCodes.Call, controlflow.ldcClass.Methods[0]));
                    }
                }
            }
            method.Body.OptimizeBranches();
        }

        public static void processMethods(IList<MethodDef> methods)
        {
            foreach (MethodDef methodDef in methods)
            {
                if (methodDef.Body != null && methodDef.Body.Instructions.Count > 2)
                {
                    if (methodDef.Body.ExceptionHandlers.Count == 0)
                    {
                        controlflow.processMethod1(methodDef);
                        if (methodDef.DeclaringType != controlflow.ldcClass)
                        {
                            controlflow.processMethod2(methodDef);
                        }
                        controlflow.processMethod1(methodDef);
                    }
                    else if (methodDef.DeclaringType != controlflow.ldcClass)
                    {
                        controlflow.processMethod2(methodDef);
                    }
                }
            }
        }

        public static void process(ModuleDefMD asm)
        {
            controlflow.Intiailize(asm);
            foreach (TypeDef typeDef in asm.Types)
            {
                controlflow.processMethods(typeDef.Methods);
            }
        }

        private static ModuleDefMD asm;

        public static bool intialized = false;

        private static TypeDef ldcClass;
    }
}