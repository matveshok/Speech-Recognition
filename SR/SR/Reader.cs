﻿using System;
using System.Collections.Generic;
using System.Text;
using NAudio;
using NAudio.Wave;

namespace SR
{
    class Reader : SignalBase
    {
        WaveFileReader _reader;
        private int position;
        private int step;

        public Reader(string fileName)
        {
            _reader = new WaveFileReader(fileName);
            _sampleRate = _reader.WaveFormat.SampleRate;
            _channels = _reader.WaveFormat.Channels;
            _length = (int)(_reader.SampleCount /*/ _channels*/);
            Console.WriteLine($"_reader.SampleCount = {_reader.SampleCount}");
            _wF = _reader.WaveFormat;

            Read();
        }

        public override void Read()
        {
            byte[] wave = new byte[_reader.Length];
            data = new Double[(wave.Length/* - 44*/) / 2];
            _reader.Read(wave, 0, Convert.ToInt32(_reader.Length));

            double i = 0;

            for (i = 0; i < data.Length; i++)
                data[(int)i] = BitConverter.ToInt16(wave,/* 44 + */(int)i * 2) / 32768.0f;

            position = 0;
            step = (int)(20.0 / (1000.0 / SampleRate));
        }

        public bool isEmpty()
        {
            if (position >= Length*Channels)
                return true;
            return false;
        }

        public double[] Next()
        {
            if (isEmpty())
                return null;

            if (position + step >= Length * Channels)
                step = Length * Channels - position;

            Console.WriteLine($"step = {step}\nposition = {position}");

            double[] dataStep = new double[step];

            for (int i = 0; i < dataStep.Length; i++)
                dataStep[i] = data[position + i];
            
            //HammingWindow hamming = new HammingWindow(dataStep);

            position += step;

            //return hamming.Data;
            return dataStep;
        }

        public void Play(string fileName)
        {
            _reader = new WaveFileReader(fileName);
            //WaveOutEvent player = new WaveOutEvent();
            NAudio.Wave.DirectSoundOut sound = new DirectSoundOut();
            sound.Init(new WaveChannel32(_reader));
            sound.Play();
        }

        public int SampleRate => _sampleRate;
        public int Channels => _channels;
        public int Length => _length;
        public WaveFormat WF => _wF;
        public double[] Data => data;

        public override void Reset(string fileName)
        {
            data = null;
            _reader = new WaveFileReader(fileName);

            Read();
        }
    }
}
