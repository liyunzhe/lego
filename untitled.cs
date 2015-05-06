using System.IO;
using System;

namespace ParticleSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            ParticleSystem particle_system = new ParticleSystem(10, 5, 5, 5, 10);

            particle_system.Update(2, 10);
            particle_system.PrintInfo();
        }
    }

    class Field
    {
        //public bool isUniform = True;

        //set the size of differentials
        private static double h = 0.0000001;

        public double Potential(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        //compute potential gradient
        public void PotentialGradient(ref double x, ref double y)
        {
            double temp_x, temp_y;
            temp_x = x; temp_y = y;
            double h2 = h * 2;
            x = (Potential(temp_x - h2, temp_y) - 8 * Potential(temp_x - h, temp_y) + 8 * Potential(temp_x + h, temp_y) - Potential(temp_x + h2, temp_y)) / (h2 * 6);
            y = (Potential(temp_x, temp_y - h2) - 8 * Potential(temp_x, temp_y - h) + 8 * Potential(temp_x, temp_y + h) - Potential(temp_x, temp_y + h2)) / (h2 * 6);
        }

    }

    class Particle
    {
        public double position_x, position_y, velocity_x, velocity_y, mass, radius;
        
        public Particle (double x, double y, double vx, double vy, double m, double r )
        {
            position_x = x;
            position_y = y;
            velocity_x = vx;
            velocity_y = vy;
            mass = m;
            radius = r;
        }

        public double Energy()
        {
            return 0.5 * mass * NetVelocitySq();
        }

        //convert a particle's info into a string for printing.
        public override string ToString()
        {
            double energy = Energy();
            return "Energy = " + energy + ", Position = " + position_x + ", " + position_y;
        }

        //compute the energy squared of the particle.
        private double NetVelocitySq()
        {
            return Math.Pow(velocity_x, 2) + Math.Pow(velocity_y, 2);
        }
    }

    class ParticleSystem
    {
        public int Max_particle_number;
        public int Particle_number;
        public double MAX_V, MAX_MASS, MAX_RADIUS, FIELD_SIZE;
        public Particle[] Particle_array;
        private static int fps = 100;
        Field potential_field = new Field();

        public ParticleSystem(int i, double V, double M, double R, double SIZE)
        {
            MAX_RADIUS = R;
            MAX_V = V;
            MAX_MASS = M;
            Max_particle_number = i;
            FIELD_SIZE = SIZE;
            Random randomizer = new Random();
            int RandomStep = 10;

            Particle_array = new Particle[Max_particle_number];

            for (int j = 0; j < Max_particle_number; j++)
            {
                
                double vx = (double)randomizer.Next(-RandomStep, RandomStep + 1) / RandomStep * MAX_V;
                double vy = (double)randomizer.Next(-RandomStep, RandomStep + 1) / RandomStep * MAX_V;
                double x = (double)randomizer.Next(-RandomStep, RandomStep + 1) / RandomStep * FIELD_SIZE;
                double y = (double)randomizer.Next(-RandomStep, RandomStep + 1) / RandomStep * FIELD_SIZE;
                double m = (double)randomizer.Next(1, RandomStep + 1) / RandomStep * MAX_MASS;
                double r = (double)randomizer.Next(1, RandomStep + 1) / RandomStep * MAX_RADIUS;

                Create_particle(j, x, y, vx, vy, m, r);
                Particle_number++;
            }

            Console.WriteLine("{0} particles created.", Particle_number);

            //set to 100 for testing first...
            //fps = 100;
        }

        //insert a particle at index i in the array
        private void Create_particle(int i, double x, double y, double vx, double vy, double m, double r)
        {
            Particle new_particle = new Particle(x, y, vx, vy, m, r);
            Particle_array[i] = new_particle;
            Console.WriteLine(new_particle.ToString() + " is created.");
        }


        public void PrintInfo()
        {
            foreach (Particle particle in Particle_array)
            {
                Console.WriteLine(particle.ToString());
            }
        }




        public void Update(double duration)
        {
            int num_frames = (int)(duration * fps);
            double frame_interval = (double)1 / fps;
            for (int i = 0; i < num_frames; i++)
            {
                UpdatePositionAndVelocity(frame_interval);
                HandleCollision();
                Console.WriteLine("Frame {0}: ", i);
                PrintInfo();
                Console.WriteLine();
            }
        }

        //second parameter indicate the number of unprinted frames between adjacent printed frames
        public void Update(double duration, int printed_frame_interval)
        {
            int num_frames = (int)duration * fps;
            double frame_interval = (double)1 / fps;
            for (int i = 0; i < num_frames; i++)
            {
                UpdatePositionAndVelocity(frame_interval);
                HandleCollision();
                if (i % printed_frame_interval == 0 && i != 0)
                {        
                    Console.WriteLine("Frame {0}: ", i / printed_frame_interval);
                    PrintInfo();
                    Console.WriteLine();   
                }
            }
        }

        void UpdatePositionAndVelocity(double duration)
        {
            foreach (Particle particle in Particle_array)
            {
                //aaceleration in x and y direction
                double acc_x = particle.position_x;
                double acc_y = particle.position_y;
                potential_field.PotentialGradient(ref acc_x, ref acc_y);

                //acceleration is the negative of potential gradient
                particle.position_x += particle.velocity_x * duration - 0.5 * acc_x * duration * duration;
                particle.position_y += particle.velocity_y * duration - 0.5 * acc_y * duration * duration;

                //update velocity, too
                particle.velocity_x += acc_x * duration;
                particle.velocity_y += acc_y * duration;
            }
        }

        void HandleCollision()
        {
            foreach (Particle particle in Particle_array)
            {
                //flip velocity and flip position around the axes..
                if (Math.Abs(particle.position_y) > FIELD_SIZE)
                {
                    particle.velocity_y *= -1;
                    if (particle.position_y < 0)
                    {
                        particle.position_y = Math.Abs(particle.position_y - FIELD_SIZE) - FIELD_SIZE;
                    }
                    else
                    {
                        particle.position_y = FIELD_SIZE - Math.Abs(particle.position_y - FIELD_SIZE);   
                    }
                }
                if (Math.Abs(particle.position_x) > FIELD_SIZE)
                {
                    particle.velocity_x *= -1;
                    if (particle.position_x < 0)
                    {
                        particle.position_x = Math.Abs(particle.position_x - FIELD_SIZE) - FIELD_SIZE;
                    }
                    else
                    {
                        particle.position_x = FIELD_SIZE - Math.Abs(particle.position_x - FIELD_SIZE);   
                    }
                }
            }
        }


    }

}