   M S B A M L   `   `   `  ����B  >ChatApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:ChatAppChatApp  5          ChatApp.ConnectionUi   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      'localclr-namespace:ChatApp     5      $	��260��5      $	��300��6     � 	Resources  5
       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $ ../GlobalDirectory.xaml?�6%   %    5      .��5      �  � RowDefinitions 5   
   �� 5      $
��auto �6   �� 5      $��* �6   .+�5   
   ��    PrimaryBlue#��[  6    �� CornerRadius$ 15��6;   $��10q�6M   ._�5      �� -   5        $��InfoText��5      $��Failed to connect!��6(   
�������$��25(�6U    �� Padding$ 10q�6c   $��Center=�6p   5      � 5   
   -   5        $��	InputGrid��5      $��1��6   $y�Visible=�6-    5      �� 5      $
��auto �6    �� 5       $
��auto �6    �� 5!      $��* �6    .+�5$      �� #��[  6   $ 15��6?   $��10q�6Q   ._�5%      �  5&      �� 5'      $
��auto �6(   �� 5(      $
��auto �6(   �� 5)      $��* �6(   .+�5,      �� -   5         ��Name$ IpInputField��5,      $��18(�64   
 � Tag$ ip��6B   $��112.5��6K    �� TextWrapping$
 Wrap=�6]   $��Stretch=�6q     SecondaryBlue#��[ 6�   
�������  
InputField#��[ 6�   $��5q�6�   �� 5-      -   5        $ PortInputField��5-   +   $��1��6   $��18(�6C   $
 port��6Q   $��112.5��6\   $
 Wrap=�6n   $��Stretch=�6�   #��[ 6�   
�������#��[ 6�   $��5q�6  5.      5/      �� 51      -   5        $��ConnectButton��51      $��2��6*   #��[  67   $ 15��6a   $��10q�6s   ._�52      �� -   5        #��[ 52      $ 10��6E   $��5q�6W   ._�53      �� $��Connect��6    
�������$��25(�6B   $ 10q�6P   $��Center=�6]   $��Center=�6z   54      55      56      57      58      