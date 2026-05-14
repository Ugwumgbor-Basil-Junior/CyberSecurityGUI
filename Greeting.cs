using System;
using System.Collections.Generic;
using System.Text;
using System.Media;
using System.IO;

namespace CyberSecurityGUI
{
    internal class Greeting
    {
        private SoundPlayer player;

        public Greeting()
        {
            player = new SoundPlayer();
        }

        public void PlayGreeting(string audioFilePath)
        {
            if (File.Exists(audioFilePath))
            {
                player.SoundLocation = audioFilePath;
                player.Play();
            }
            else
            {
                throw new FileNotFoundException($"Audio file not found at: {audioFilePath}");
            }
        }

        public void StopGreeting()
        {
            player.Stop();
        }
    }
}