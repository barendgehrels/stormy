using System;
using System.Collections.Generic;

namespace CatAndMouse
{
    public static class Common
    {
    	public static void List(IEnumerable<Cat> cats, IEnumerable<Mouse> mouses)
    	{
            foreach (Cat cat in cats)
            {
                System.Console.WriteLine("Cat {0}", cat.Name);
                foreach (Mouse mouse in cat.Chases)
                {
                    System.Console.WriteLine(" chases {0}", mouse.Name);
                }
            }
    	}

        public static void List(IEnumerable<Cat> cats)
        {
            foreach (Cat cat in cats)
            {
                System.Console.WriteLine("Cat {0}", cat.Name);
                foreach (Mouse mouse in cat.Chases)
                {
                    System.Console.WriteLine(" chases {0}", mouse.Name);
                }
            }
        }

    }
}
