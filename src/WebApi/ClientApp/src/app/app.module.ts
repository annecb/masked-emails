
import { AppRoutingModule } from './app-routing.module';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { AuthModule, ConfigResult, OidcConfigService, OidcSecurityService } from 'angular-auth-oidc-client';
import { BrowserModule } from '@angular/platform-browser';
import { CoreModule } from './core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { AddressesComponent } from './addresses/addresses.component';
import { MaskedEmailsComponent } from './masked-emails/masked-emails.component';
import { MyProfileComponent } from './my-profile/my-profile.component';

import { AuthorizationGuard } from './core/authorization-guard';
import { environment } from '../environments/environment';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

//Font Awesome Module
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { library } from '@fortawesome/fontawesome-svg-core';
import { fas } from '@fortawesome/free-solid-svg-icons';

//Import material module
import { MaterialModule } from './material.module';
import { LoginComponent } from './login/login.component'
import { GravatarModule } from 'ngx-gravatar';

export function loadConfig(oidcConfigService: OidcConfigService) {
  return () => {
    const oidc_configuration = 'assets/auth.clientConfiguration.json';
    const oidc_configuration_prod = 'assets/auth.clientConfiguration.prod.json';

    var configuration = oidc_configuration;
    if (environment.production)
      configuration = oidc_configuration_prod;

    console.log(`loading ${configuration} OpenId configuration.`);

    oidcConfigService.load(configuration);
  }
}

@NgModule({
  declarations: [
    AddressesComponent,
    AppComponent,
    MaskedEmailsComponent,
    MyProfileComponent,
    LoginComponent,
  ],
  imports: [
    AppRoutingModule,
    AuthModule.forRoot(),
    BrowserModule,
    CoreModule,
    FormsModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MaterialModule,
    GravatarModule,
    FontAwesomeModule
  ],
  providers: [
    AuthorizationGuard,
    OidcConfigService,
    {
      provide: APP_INITIALIZER,
      useFactory: loadConfig,
      deps: [OidcConfigService],
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {
  constructor(private openId: OidcSecurityService, private configService: OidcConfigService) {
    library.add(fas);

    this.configService.onConfigurationLoaded.subscribe((configResult: ConfigResult) => {

      console.log(configResult.customConfig);

      this.openId.setupModule(
        configResult.customConfig,
        configResult.authWellknownEndpoints
      );
    });
  }
}
