/* Copyright 2019 Vincent Bousquet

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
   */
using FlexDMD.Actors;
using NLog;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace FlexDMD
{
    class GIFImage : AnimatedActor
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const int PropertyTagFrameDelay = 0x5100;
        private readonly AssetSrc _src;
        private readonly AssetManager _manager;
        private readonly float _prefWidth, _prefHeight;
        private readonly float[] _frameDelays;
        private readonly float _length;
        private CachedBitmap _cachedBitmap = null;
        private int _pos = 0;

        public GIFImage(AssetManager manager, string path, string name = "")
        {
            _src = manager.ResolveSrc(path);
            _manager = manager;
            Name = name;
            // Initialize by loading the frame from the asset manager.
            _cachedBitmap = _manager.GetBitmap(_src);
            _prefWidth = _cachedBitmap.Width;
            _prefHeight = _cachedBitmap.Height;
            var item = _cachedBitmap.Bitmap.GetPropertyItem(PropertyTagFrameDelay);
            _frameDelays = new float[_cachedBitmap.Bitmap.GetFrameCount(FrameDimension.Time)];
            if (item.Type != 4)
            {
                log.Error("Invalid GIF: frame delays are not of type 4 (32 bits unsigned int) but {0}", item.Type);
                _cachedBitmap = null;
                return;
            }
            var length = 0f;
            for (int i = 0; i < _frameDelays.Length; i++)
            {
                _frameDelays[i] = BitConverter.ToInt32(item.Value, i * 4) / 100.0f;
                length += _frameDelays[i];
            }
            _length = length;
            Rewind();
            Pack();
            // Since we are not on stage, we can not guarantee that the data will remain live in memory
            _cachedBitmap.Bitmap = null;
        }

        public override float PrefWidth { get => _prefWidth; }
        public override float PrefHeight { get => _prefHeight; }
        public override float Length { get => _length; }

        protected override void OnStageStateChanged()
        {
            // Data returned by the asset manager are only valid when on stage
            _cachedBitmap = OnStage ? _manager.GetBitmap(_src) : null;
          //  _bitmap = OnStage ? _manager.GetBitmap(_src).Bitmap : null;
            UpdateFrame();
        }

        protected override void Rewind()
        {
            base.Rewind();
            _pos = 0;
            _frameDuration = _frameDelays[0];
        }

        protected override void ReadNextFrame()
        {
            if (_pos >= _frameDelays.Length - 1)
            {
                _endOfAnimation = true;
            }
            else
            {
                _pos++;
                _frameTime = 0;
                for (int i = 0; i < _pos; i++)
                    _frameTime += _frameDelays[i];
                _frameDuration = _frameDelays[_pos];
                UpdateFrame();
            }
        }

        private void UpdateFrame()
        {
            if (_cachedBitmap != null)
            {
                _cachedBitmap.Bitmap.SelectActiveFrame(FrameDimension.Time, _pos);
                Rectangle rect = new Rectangle(0, 0, _cachedBitmap.Width, _cachedBitmap.Height);
                BitmapData data = _cachedBitmap.Bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                GraphicUtils.BGRtoRGB(data.Scan0, data.Stride, _cachedBitmap.Width, _cachedBitmap.Height);
                _cachedBitmap.Bitmap.UnlockBits(data);
            }
        }

        public override void Draw(Graphics graphics)
        {
            if (Visible && _cachedBitmap != null)
            {
                Layout.Scale(Scaling, PrefWidth, PrefHeight, Width, Height, out float w, out float h);
                Layout.Align(Alignment, w, h, Width, Height, out float x, out float y);
                graphics.DrawImage(_cachedBitmap.Bitmap, (int)(X + x), (int)(Y + y), (int)w, (int)h);
            }
        }
    }
}

