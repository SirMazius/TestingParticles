using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilidades {

	public static int Next_prime(int number)
    {
        bool found = false;
        int prime = number;

        while (!found)
        {
            bool searching = true;
            prime++;

            if (prime == 2 || prime == 3)
            {
                found = true;
                return prime;
            }

            if (prime % 2 == 0 || prime % 3 == 0)
            {
                found = false;
                searching = false;
            }


            int divisor = 6;

            while (searching && divisor * divisor - 2 * divisor + 1 <= prime)
            {
                if (prime % (divisor - 1) == 0 || prime % (divisor + 1) == 0)
                {
                    found = false;
                    searching = false;
                }


                divisor += 6;
            }

            if (searching)
            {
                found = true;
            }

        }

        return prime;
    }

}
