﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Collections.Generic;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Gui;
using ClassicalSharp.Renderers;

namespace ClassicalSharp {

	public sealed class GuiInterface : IGameComponent {
		
		public int GuiTex, GuiClassicTex, IconsTex;
		Game game;
		IGraphicsApi api;
		FpsScreen fpsScreen;
		internal HudScreen hudScreen;
		internal Screen activeScreen;
		internal List<WarningScreen> overlays = new List<WarningScreen>();
		
		public GuiInterface( Game game ) {
			fpsScreen = game.AddComponent( new FpsScreen( game ) );
			hudScreen = game.AddComponent( new HudScreen( game ) );
		}
		
		/// <summary> Gets the screen that the user is currently interacting with. </summary>
		public Screen ActiveScreen {
			get { return overlays.Count > 0 ? overlays[0]
					: activeScreen == null ? hudScreen : activeScreen; }
		}
		
		public void OnNewMap( Game game ) { }
		public void OnNewMapLoaded( Game game ) { }
		public void Ready( Game game ) { }
		
		public void Init( Game game ) {
			this.game = game;
			api = game.Graphics;
		}
		
		public void Reset( Game game ) {
			foreach( WarningScreen screen in overlays )
				screen.Dispose();
			overlays.Clear();
		}
		
		public void Dispose() {
			SetNewScreen( null );
			fpsScreen.Dispose();
			
			if( activeScreen != null )
				activeScreen.Dispose();
			api.DeleteTexture( ref GuiTex );
			api.DeleteTexture( ref GuiClassicTex );
			api.DeleteTexture( ref IconsTex );			
			
			foreach( WarningScreen screen in overlays )
				screen.Dispose();
		}
		
		public void SetNewScreen( Screen screen ) { SetNewScreen( screen, true ); }
		
		public void SetNewScreen( Screen screen, bool disposeOld ) {
			game.InputHandler.ScreenChanged( activeScreen, screen );
			if( activeScreen != null && disposeOld )
				activeScreen.Dispose();
			
			if( screen == null ) {
				hudScreen.GainFocus();
			} else if( activeScreen == null ) {
				hudScreen.LoseFocus();
			}
			
			if( screen != null )
				screen.Init();
			activeScreen = screen;
		}
		
		public void RefreshHud() { hudScreen.Recreate(); }
		
		public void ShowWarning( WarningScreen screen ) {
			bool cursorVis = game.CursorVisible;
			if( overlays.Count == 0 ) game.CursorVisible = true;
			overlays.Add( screen );
			if( overlays.Count == 1 ) game.CursorVisible = cursorVis;
			// Save cursor visibility state
			screen.Init();
		}
		
		
		public void Render( double delta ) {
			api.Mode2D( game.Width, game.Height, game.EnvRenderer is StandardEnvRenderer );
			if( activeScreen == null || !activeScreen.HidesHud )
				fpsScreen.Render( delta );
			
			if( activeScreen == null || !activeScreen.HidesHud && !activeScreen.RenderHudAfter )
				hudScreen.Render( delta );
			if( activeScreen != null )
				activeScreen.Render( delta );
			if( activeScreen != null && !activeScreen.HidesHud && activeScreen.RenderHudAfter )
				hudScreen.Render( delta );
			
			if( overlays.Count > 0 )
				overlays[0].Render( delta );
			api.Mode3D( game.EnvRenderer is StandardEnvRenderer );
		}
		
		internal void OnResize( int oWidth, int oHeight ) {			
			if( activeScreen != null )
				activeScreen.OnResize( oWidth, oHeight, game.Width, game.Height );
			hudScreen.OnResize( oWidth, oHeight, game.Width, game.Height );
			foreach( Screen overlay in overlays )
				overlay.OnResize( oWidth, oHeight, game.Width, game.Height );
		}
	}
}