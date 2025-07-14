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
using NLog;
using NLog.Fluent;
using System;
using System.Drawing;

namespace FlexDMD
{
    public class Image : Actor, IImageActor
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly AssetSrc _src;
        private readonly AssetManager _manager;
        private readonly float _prefWidth, _prefHeight;
        private CachedBitmap _cachedBitmap = null;
        // private Bitmap _bitmap = null;

        public Scaling Scaling { get; set; } = Scaling.Stretch;
        public Alignment Alignment { get; set; } = Alignment.Center;
        public override float PrefWidth { get => _prefWidth; }
        public override float PrefHeight { get => _prefHeight; }

        public Image(AssetManager manager, string path, string name = "")
        {
            try
            {
                log.Info($"Creating Image actor with path '{path}' and name '{name}'");
                _src = manager.ResolveSrc(path);
                _manager = manager;
                Name = name;
                // Initialize by loading the frame from the asset manager.
                _cachedBitmap = _manager.GetBitmap(_src);
                //_bitmap = _cachedBitmap.Bitmap;
                _prefWidth = _cachedBitmap.Width;
                _prefHeight = _cachedBitmap.Height;
                Pack();
                // Since we are not on stage, we can not guarantee that the data will remain live in memory
                //_bitmap = null;
                _cachedBitmap = null;
            }
            catch (Exception e)
            {
                log.Error(e, $"Failed to load image: {e.Message}");
            }
        }

        /// <summary>
        /// Allows to directly acces the bitmap. This bypass the default lifecycle management (tied to FlexDMD run state 
        /// and actors being on stage) and needs the user to perform it.
        /// </summary>
        public Bitmap Bitmap
        {
            get
            {
                if (_cachedBitmap == null) _cachedBitmap = _manager.GetBitmap(_src);
                return _cachedBitmap.Bitmap;
            }
            set
            {
                _cachedBitmap.Bitmap = value;
            }
        }
        protected override void OnStageStateChanged()
        {
            //_bitmap = OnStage ? _manager.GetBitmap(_src).Bitmap : null;
            //_cachedBitmap.Bitmap = OnStage ? _manager.GetBitmap(_src).Bitmap : null;
            _cachedBitmap = OnStage ? _manager.GetBitmap(_src) : null;
        }

        public override void Draw(Graphics graphics)
        {
            if (Visible && _cachedBitmap.Bitmap != null)
            {
                Layout.Scale(Scaling, PrefWidth, PrefHeight, Width, Height, out float w, out float h);
                Layout.Align(Alignment, w, h, Width, Height, out float x, out float y);
                graphics.DrawImage(_cachedBitmap.Bitmap, (int)(X + x), (int)(Y + y), (int)w, (int)h);
            }
        }
    }
}

